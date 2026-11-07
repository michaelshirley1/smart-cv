using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using SmartCv.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SmartCv.Services;

public interface IDocumentReader
{
    IReadOnlyCollection<string> Extensions { get; }
    IReadOnlyList<ImportedParagraph> Read(Stream content);
}

public class DocxReader : IDocumentReader
{
    private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    public IReadOnlyCollection<string> Extensions { get; } = [".docx"];

    public IReadOnlyList<ImportedParagraph> Read(Stream content)
    {
        XDocument document;
        try
        {
            using var zip = new ZipArchive(content, ZipArchiveMode.Read);
            var entry = zip.GetEntry("word/document.xml") ?? throw new ImportException("That file isn't a Word document.");
            using var xml = entry.Open();
            document = XDocument.Load(xml);
        }
        catch (Exception ex) when (ex is InvalidDataException or XmlException)
        {
            throw new ImportException("That Word document couldn't be read.", ex);
        }

        var body = document.Root?.Element(W + "body");
        if (body is null) return [];

        return body.Descendants(W + "p").Select(ToParagraph).OfType<ImportedParagraph>().ToList();
    }

    private static ImportedParagraph? ToParagraph(XElement paragraph)
    {
        var properties = paragraph.Element(W + "pPr");
        var style = properties?.Element(W + "pStyle")?.Attribute(W + "val")?.Value ?? string.Empty;
        var isBullet = properties?.Element(W + "numPr") is not null || style.Contains("List", StringComparison.OrdinalIgnoreCase);

        var runs = new List<(string Text, bool Bold)>();
        foreach (var run in paragraph.Descendants(W + "r"))
        {
            var bold = IsBold(run.Element(W + "rPr"));
            foreach (var child in run.Elements())
            {
                if (child.Name == W + "t") runs.Add((child.Value, bold));
                else if (child.Name == W + "tab") runs.Add(("\t", false));
                else if (child.Name == W + "br" || child.Name == W + "cr") runs.Add((" ", false));
            }
        }

        var firstTab = runs.FindIndex(run => run.Text == "\t");
        var left = firstTab < 0 ? runs : runs.Take(firstTab).ToList();
        var right = firstTab < 0 ? string.Empty : string.Concat(runs.Skip(firstTab).Select(run => run.Text)).Replace('\t', ' ').Trim();

        var text = RunMarkup.Build(left);
        if (text.Length == 0)
        {
            if (right.Length == 0) return null;
            text = right;
            right = string.Empty;
        }

        return new ImportedParagraph
        {
            Text = text,
            RightText = right.Length > 0 ? right : null,
            IsBullet = isBullet,
            IsBold = RunMarkup.IsBold(left),
        };
    }

    private static bool IsBold(XElement? runProperties)
    {
        var bold = runProperties?.Element(W + "b");
        var value = bold?.Attribute(W + "val")?.Value;
        return bold is not null && value is not ("0" or "false" or "off");
    }
}

public class PdfReader : IDocumentReader
{
    private const double ColumnGap = 30;
    private static readonly string[] BulletMarks = ["▪", "•", "●", "○", "◦", "■", "►", "‣", "∙", "·", "-", "–", "*"];
    private static readonly string[] BoldMarks = ["bold", "black", "heavy", "semibold", "demi"];

    public IReadOnlyCollection<string> Extensions { get; } = [".pdf"];

    private sealed class VisualLine
    {
        public List<(string Text, bool Bold)> Runs { get; } = [];
        public List<(string Text, bool Bold)> RightRuns { get; } = [];
        public bool IsBullet { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Baseline { get; set; }
        public double FontSize { get; set; }
    }

    public IReadOnlyList<ImportedParagraph> Read(Stream content)
    {
        using var buffer = new MemoryStream();
        content.CopyTo(buffer);

        var lines = new List<VisualLine>();
        try
        {
            using var document = PdfDocument.Open(buffer.ToArray());
            foreach (var page in document.GetPages())
            {
                lines.AddRange(LinesOf(page));
            }
        }
        catch (Exception ex) when (ex is not ImportException)
        {
            throw new ImportException("That PDF couldn't be read.", ex);
        }

        return Merge(lines);
    }

    private static IEnumerable<VisualLine> LinesOf(Page page)
    {
        var words = page.GetWords().Where(word => !string.IsNullOrWhiteSpace(word.Text)).ToList();
        var rows = new List<List<Word>>();
        foreach (var word in words.OrderByDescending(Baseline).ThenBy(word => word.BoundingBox.Left))
        {
            var row = rows.FirstOrDefault(candidate => Math.Abs(Baseline(candidate[0]) - Baseline(word)) <= Math.Max(2, word.BoundingBox.Height * 0.3));
            if (row is null) rows.Add([word]);
            else row.Add(word);
        }

        foreach (var row in rows.OrderByDescending(candidate => Baseline(candidate[0])))
        {
            var ordered = row.OrderBy(word => word.BoundingBox.Left).ToList();
            if (ordered.All(word => word.Text.All(char.IsDigit))) continue;

            var line = new VisualLine
            {
                Baseline = Baseline(ordered[0]),
                FontSize = ordered.SelectMany(word => word.Letters).Average(letter => letter.PointSize),
                Right = ordered[^1].BoundingBox.Right,
            };

            var first = ordered[0].Text;
            var bullet = BulletMarks.FirstOrDefault(first.StartsWith);
            if (bullet is not null)
            {
                line.IsBullet = true;
                if (first.Length == bullet.Length) ordered.RemoveAt(0);
            }

            if (ordered.Count == 0) continue;
            line.Left = ordered[0].BoundingBox.Left;

            var target = line.Runs;
            for (var index = 0; index < ordered.Count; index++)
            {
                var word = ordered[index];
                if (index > 0 && word.BoundingBox.Left - ordered[index - 1].BoundingBox.Right > ColumnGap) target = line.RightRuns;
                if (target.Count > 0) target.Add((" ", false));

                var letters = word.Letters.ToList();
                if (index == 0 && line.IsBullet && bullet is not null && word.Text.StartsWith(bullet)) letters = letters.Skip(bullet.Length).ToList();
                for (var position = 0; position < letters.Count; position++)
                {
                    var letter = letters[position];
                    var gap = position == 0 ? 0 : letter.StartBaseLine.X - letters[position - 1].EndBaseLine.X;
                    if (gap > letter.PointSize * 0.2) target.Add((" ", false));
                    target.Add((letter.Value, IsBold(letter)));
                }
            }

            yield return line;
        }
    }

    private static List<ImportedParagraph> Merge(List<VisualLine> lines)
    {
        var paragraphs = new List<(VisualLine First, VisualLine Last, List<(string Text, bool Bold)> Runs)>();
        var contentRight = lines.Count == 0 ? 0 : lines.Max(line => line.Right);

        foreach (var line in lines)
        {
            if (paragraphs.Count > 0 && Continues(paragraphs[^1].First, paragraphs[^1].Last, line, contentRight))
            {
                var previous = paragraphs[^1];
                previous.Runs.Add((" ", false));
                previous.Runs.AddRange(line.Runs);
                paragraphs[^1] = (previous.First, line, previous.Runs);
            }
            else
            {
                paragraphs.Add((line, line, [.. line.Runs]));
            }
        }

        return paragraphs
            .Select(paragraph => new ImportedParagraph
            {
                Text = RunMarkup.Build(paragraph.Runs),
                RightText = paragraph.First.RightRuns.Count > 0 ? RunMarkup.Build(paragraph.First.RightRuns).Replace("**", string.Empty) : null,
                IsBullet = paragraph.First.IsBullet,
                IsBold = RunMarkup.IsBold(paragraph.Runs),
            })
            .Where(paragraph => paragraph.Text.Length > 0)
            .ToList();
    }

    private static bool Continues(VisualLine first, VisualLine previous, VisualLine line, double contentRight)
    {
        if (line.IsBullet || line.RightRuns.Count > 0 || previous.RightRuns.Count > 0) return false;

        var gap = previous.Baseline - line.Baseline;
        var lineHeight = Math.Max(previous.FontSize, line.FontSize) * 1.6;
        if (gap <= 0 || gap > lineHeight || Math.Abs(previous.FontSize - line.FontSize) > 1) return false;

        if (first.IsBullet) return line.Left >= first.Left - 3;
        if (line.Runs.Count > 0 && line.Runs[0].Bold) return false;

        var previousText = string.Concat(previous.Runs.Select(run => run.Text));
        return previous.Right >= contentRight * 0.85 && !previousText.Contains('@');
    }

    private static double Baseline(Word word) => Math.Round(word.Letters[0].StartBaseLine.Y, 1);

    private static bool IsBold(Letter letter) =>
        BoldMarks.Any(mark => (letter.FontName ?? string.Empty).Contains(mark, StringComparison.OrdinalIgnoreCase));
}

public class PlainTextReader : IDocumentReader
{
    private static readonly string[] BulletMarks = ["- ", "* ", "• ", "▪ "];

    public IReadOnlyCollection<string> Extensions { get; } = [".txt", ".md"];

    public IReadOnlyList<ImportedParagraph> Read(Stream content)
    {
        using var reader = new StreamReader(content);
        var paragraphs = new List<ImportedParagraph>();

        foreach (var raw in reader.ReadToEnd().Split('\n'))
        {
            var line = raw.TrimEnd('\r').Trim();
            if (line.Length == 0 || line.All(character => character is '-' or '=' or '_')) continue;

            var isHeading = line.StartsWith('#');
            var bullet = BulletMarks.FirstOrDefault(line.StartsWith);
            var text = isHeading ? line.TrimStart('#').Trim().ToUpperInvariant() : bullet is not null ? line[bullet.Length..].Trim() : line;
            var parts = text.Split('\t', 2, StringSplitOptions.TrimEntries);

            paragraphs.Add(new ImportedParagraph
            {
                Text = parts[0],
                RightText = parts.Length > 1 && parts[1].Length > 0 ? parts[1] : null,
                IsBullet = bullet is not null,
                IsBold = isHeading || (parts[0].StartsWith("**") && parts[0].EndsWith("**") && parts[0].Length > 4),
            });
        }

        return paragraphs;
    }
}
