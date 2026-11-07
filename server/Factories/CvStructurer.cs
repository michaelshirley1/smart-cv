using System.Globalization;
using System.Text.RegularExpressions;
using SmartCv.Models;

namespace SmartCv.Factories;

public static partial class CvStructurer
{
    private const int MaxHeadingLength = 40;
    private const int MaxContactLines = 3;

    private static readonly (string[] Keys, SectionKind Kind)[] KindKeys =
    [
        (["summary", "profile", "about", "objective", "overview"], SectionKind.Summary),
        (["experience", "employment", "work history", "career"], SectionKind.Experience),
        (["education", "qualification", "academic"], SectionKind.Education),
        (["project"], SectionKind.Projects),
        (["skill", "technolog", "tools", "competenc", "expertise", "tech stack"], SectionKind.Skills),
    ];

    private static readonly string[] OtherHeadings =
    [
        "interests", "hobbies", "certifications", "certificates", "awards", "achievements", "languages",
        "publications", "volunteering", "volunteer", "references", "activities", "courses", "training",
    ];

    public static ImportedCv Structure(IReadOnlyList<ImportedParagraph> paragraphs)
    {
        var cv = new ImportedCv();
        var index = 0;

        if (paragraphs.Count > 0 && !IsContact(paragraphs[0]) && !IsHeading(paragraphs[0]))
        {
            cv.FullName = Plain(paragraphs[0].Text);
            index = 1;
        }

        for (var contactLines = 0; index < paragraphs.Count && contactLines < MaxContactLines && IsContact(paragraphs[index]) && !IsHeading(paragraphs[index]); index++, contactLines++)
        {
            AddContact(cv, Plain(paragraphs[index].Text));
        }

        ImportedSection? section = null;
        ImportedEntry? entry = null;
        var expectingRole = false;

        for (; index < paragraphs.Count; index++)
        {
            var paragraph = paragraphs[index];

            if (IsHeading(paragraph))
            {
                var title = TitleCase(Plain(paragraph.Text).TrimEnd(':'));
                section = new ImportedSection { Title = title, Kind = KindFor(title) };
                cv.Sections.Add(section);
                entry = null;
                expectingRole = false;
                continue;
            }

            if (section is null)
            {
                section = new ImportedSection { Title = "Summary", Kind = SectionKind.Summary };
                cv.Sections.Add(section);
            }

            if (section.Kind is not (SectionKind.Experience or SectionKind.Education or SectionKind.Projects))
            {
                SingleEntry(section).Lines.Add(LineText(paragraph.Text));
                continue;
            }

            if (expectingRole && entry is not null && !paragraph.IsBullet && !paragraph.IsBold && !HasDates(paragraph))
            {
                entry.Subheading = Plain(paragraph.Text);
                entry.Location = Plain(paragraph.RightText ?? string.Empty);
                expectingRole = false;
                continue;
            }

            if (!paragraph.IsBullet && IsEntryStart(paragraph))
            {
                var (heading, dates) = SplitDates(paragraph);
                entry = new ImportedEntry { Heading = Plain(heading), DateRange = dates };
                section.Entries.Add(entry);
                expectingRole = true;
                continue;
            }

            expectingRole = false;
            if (entry is null)
            {
                entry = new ImportedEntry();
                section.Entries.Add(entry);
            }
            entry.Lines.Add(LineText(paragraph.Text));
        }

        cv.Sections.RemoveAll(candidate => candidate.Entries.All(item => item.Lines.Count == 0 && item.Heading.Length == 0));
        return cv;
    }

    public static string? DateAtEnd(string text)
    {
        var match = TrailingDates().Match(text);
        return match.Success && match.Index > 0 ? match.Groups["dates"].Value.Trim() : null;
    }

    private static bool HasDates(ImportedParagraph paragraph) =>
        DateAtEnd(Plain(paragraph.Text)) is not null
        || (!string.IsNullOrWhiteSpace(paragraph.RightText) && DateAtEnd($"x {Plain(paragraph.RightText)}") is not null);

    private static bool IsEntryStart(ImportedParagraph paragraph) =>
        HasDates(paragraph)
        || !string.IsNullOrWhiteSpace(paragraph.RightText)
        || (paragraph.IsBold && Plain(paragraph.Text).Length <= 80);

    private static (string Heading, string Dates) SplitDates(ImportedParagraph paragraph)
    {
        if (!string.IsNullOrWhiteSpace(paragraph.RightText)) return (paragraph.Text, paragraph.RightText.Trim());

        var plain = Plain(paragraph.Text);
        var dates = DateAtEnd(plain);
        return dates is null ? (plain, string.Empty) : (plain[..plain.LastIndexOf(dates, StringComparison.Ordinal)].TrimEnd(' ', ',', '|', '-', '–'), dates);
    }

    private static ImportedEntry SingleEntry(ImportedSection section)
    {
        if (section.Entries.Count == 0) section.Entries.Add(new ImportedEntry());
        return section.Entries[0];
    }

    private static bool IsHeading(ImportedParagraph paragraph)
    {
        var plain = Plain(paragraph.Text).TrimEnd(':');
        if (paragraph.IsBullet || !string.IsNullOrWhiteSpace(paragraph.RightText)) return false;
        if (plain.Length is < 2 or > MaxHeadingLength || plain.EndsWith('.') || DateAtEnd(plain) is not null) return false;
        return IsKnownHeading(plain) || IsAllCaps(plain);
    }

    private static bool IsKnownHeading(string text)
    {
        var lower = text.ToLowerInvariant();
        var words = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        return words <= 4 && (OtherHeadings.Any(lower.Contains) || KindKeys.SelectMany(pair => pair.Keys).Any(lower.Contains));
    }

    private static bool IsContact(ImportedParagraph paragraph)
    {
        var plain = Plain(paragraph.Text);
        return plain.Length < 250 && (plain.Contains('@') || WebAddress().IsMatch(plain) || Phone().IsMatch(plain));
    }

    private static void AddContact(ImportedCv cv, string text)
    {
        foreach (var item in ContactSeparator().Split(text).Select(part => part.Trim()).Where(part => part.Length > 0))
        {
            if (item.Contains('@') && !item.Contains(' ') && cv.Email.Length == 0) cv.Email = item;
            else if (Phone().IsMatch(item) && Phone().Match(item).Length >= item.Length - 2 && cv.Phone.Length == 0) cv.Phone = item;
            else if (item.Contains('.') && !item.Contains(' ')) cv.Links.Add(item);
            else if (cv.Location.Length == 0) cv.Location = item;
        }
    }

    private static SectionKind KindFor(string title)
    {
        var lower = title.ToLowerInvariant();
        foreach (var (keys, kind) in KindKeys)
        {
            if (keys.Any(lower.Contains)) return kind;
        }
        return SectionKind.Custom;
    }

    private static string LineText(string text) => LeadingBullet().Replace(text, string.Empty).Trim();

    private static string Plain(string text) => text.Replace("**", string.Empty).Trim();

    private static bool IsAllCaps(string text) =>
        text.Count(char.IsLetter) >= 3 && !text.Any(char.IsLower);

    private static string TitleCase(string text) =>
        IsAllCaps(text) ? CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToLowerInvariant()) : text;

    [GeneratedRegex(@"(?<dates>(?:(?:graduated|since|from)\s+)?(?:(?:jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec)[a-z]*\.?\s+)?\d{4}(?:\s*(?:–|—|-|to)\s*(?:(?:(?:jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec)[a-z]*\.?\s+)?\d{4}|present|current|now|today))?)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex TrailingDates();

    [GeneratedRegex(@"https?://|www\.|linkedin|github|gitlab", RegexOptions.IgnoreCase)]
    private static partial Regex WebAddress();

    [GeneratedRegex(@"\+?\d[\d\s().-]{7,}\d")]
    private static partial Regex Phone();

    [GeneratedRegex(@"\s*[❖|•·▪◆]\s*|\s{2,}")]
    private static partial Regex ContactSeparator();

    [GeneratedRegex(@"^[▪•●○◦■►‣∙·*–-]\s+")]
    private static partial Regex LeadingBullet();
}
