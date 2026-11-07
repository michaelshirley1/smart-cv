using System.IO.Compression;
using System.Text;
using SmartCv.Factories;
using SmartCv.Models;
using SmartCv.Services;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace SmartCv.Tests;

public class ImportTests
{
    private static ImportedParagraph P(string text, string? right = null, bool bullet = false, bool bold = false) =>
        new() { Text = text, RightText = right, IsBullet = bullet, IsBold = bold };

    [Fact]
    public void Structure_FindsTheShapeOfACv()
    {
        var cv = CvStructurer.Structure(
        [
            P("**Alex Smith**", bold: true),
            P("alex@example.com ❖ +64 21 555 0100 ❖ github.com/alex | Auckland"),
            P("Engineer who builds things."),
            P("**WORK EXPERIENCE**", bold: true),
            P("**Acme Ltd**", "Jan 2020 – Present", bold: true),
            P("Senior Developer", "Remote"),
            P("Built **APIs** in C#.", bullet: true),
            P("Led a team of four.", bullet: true),
            P("Beta Corp Mar 2018 – Dec 2019"),
            P("Developer"),
            P("Shipped things.", bullet: true),
            P("Skills"),
            P("**Frontend:** React, TypeScript"),
            P("EDUCATION"),
            P("**University of Auckland**", "Graduated Sep 2022", bold: true),
            P("BSc Computer Science"),
            P("CERTIFICATIONS"),
        ]);

        Assert.Equal("Alex Smith", cv.FullName);
        Assert.Equal("alex@example.com", cv.Email);
        Assert.Equal("+64 21 555 0100", cv.Phone);
        Assert.Equal(["github.com/alex"], cv.Links);
        Assert.Equal("Auckland", cv.Location);
        Assert.Equal(["Summary", "Work Experience", "Skills", "Education"], cv.Sections.Select(s => s.Title));
        Assert.Equal([SectionKind.Summary, SectionKind.Experience, SectionKind.Skills, SectionKind.Education], cv.Sections.Select(s => s.Kind));

        var jobs = cv.Sections[1].Entries;
        Assert.Equal(("Acme Ltd", "Senior Developer", "Jan 2020 – Present", "Remote"), (jobs[0].Heading, jobs[0].Subheading, jobs[0].DateRange, jobs[0].Location));
        Assert.Equal(["Built **APIs** in C#.", "Led a team of four."], jobs[0].Lines);
        Assert.Equal(("Beta Corp", "Developer", "Mar 2018 – Dec 2019"), (jobs[1].Heading, jobs[1].Subheading, jobs[1].DateRange));
        Assert.Equal(["**Frontend:** React, TypeScript"], cv.Sections[2].Entries.Single().Lines);
        Assert.Equal(("University of Auckland", "BSc Computer Science", "Graduated Sep 2022"), (cv.Sections[3].Entries[0].Heading, cv.Sections[3].Entries[0].Subheading, cv.Sections[3].Entries[0].DateRange));
    }

    [Theory]
    [InlineData("Acme Sep 2024 – Present", "Sep 2024 – Present")]
    [InlineData("Acme 2019 - 2021", "2019 - 2021")]
    [InlineData("University Graduated September 2022", "Graduated September 2022")]
    [InlineData("Built 300 things", null)]
    public void DateAtEnd_RecognisesCommonFormats(string text, string? expected)
    {
        Assert.Equal(expected, CvStructurer.DateAtEnd(text));
    }

    [Fact]
    public void DocxReader_KeepsBoldBulletsAndTabbedDates()
    {
        const string body = """
            <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>Acme Ltd</w:t></w:r><w:r><w:tab/><w:t>Jan 2020 – Present</w:t></w:r></w:p>
            <w:p><w:pPr><w:numPr><w:ilvl w:val="0"/><w:numId w:val="1"/></w:numPr></w:pPr>
              <w:r><w:t xml:space="preserve">Cut costs </w:t></w:r><w:r><w:rPr><w:b w:val="1"/></w:rPr><w:t>by 50%</w:t></w:r><w:r><w:rPr><w:b w:val="0"/></w:rPr><w:t xml:space="preserve"> overall.</w:t></w:r></w:p>
            <w:p><w:r><w:t></w:t></w:r></w:p>
            """;
        using var docx = Docx(body);

        var paragraphs = new DocxReader().Read(docx);

        Assert.Equal(2, paragraphs.Count);
        Assert.Equal(("**Acme Ltd**", "Jan 2020 – Present", false, true), (paragraphs[0].Text, paragraphs[0].RightText, paragraphs[0].IsBullet, paragraphs[0].IsBold));
        Assert.Equal(("Cut costs **by 50%** overall.", (string?)null, true, false), (paragraphs[1].Text, paragraphs[1].RightText, paragraphs[1].IsBullet, paragraphs[1].IsBold));
    }

    [Fact]
    public void DocxReader_RejectsFilesThatArentWordDocuments()
    {
        using var notZip = new MemoryStream(Encoding.UTF8.GetBytes("hello"));

        Assert.Throws<ImportException>(() => new DocxReader().Read(notZip));
    }

    [Fact]
    public void PdfReader_RebuildsLinesColumnsAndWrappedBullets()
    {
        var builder = new PdfDocumentBuilder();
        var regular = builder.AddStandard14Font(Standard14Font.Helvetica);
        var bold = builder.AddStandard14Font(Standard14Font.HelveticaBold);
        var page = builder.AddPage(PageSize.A4);
        var y = 780.0;
        void Text(string text, PdfDocumentBuilder.AddedFont font, double x) => page.AddText(text, 11, new PdfPoint(x, y), font);
        void Next() => y -= 14;

        Text("Acme Ltd", bold, 50); Text("Jan 2020 - Present", bold, 450); Next();
        Text("Senior Developer", regular, 50); Next();
        Text("-", regular, 50); Text("Built a very long line of work that goes all the way across the page so", regular, 62); Next();
        Text("that it wraps onto a second line here.", regular, 62); Next();
        Text("Frontend:", bold, 50); Text("React, TypeScript, JavaScript, HTML, CSS and a long list of other things", regular, 105); Next();
        Text("Backend:", bold, 50); Text("C#, .NET", regular, 104); Next();
        Text("3", regular, 290);
        using var pdf = new MemoryStream(builder.Build());

        var paragraphs = new PdfReader().Read(pdf);

        Assert.Equal(5, paragraphs.Count);
        Assert.Equal(("**Acme Ltd**", "Jan 2020 - Present", true), (paragraphs[0].Text, paragraphs[0].RightText, paragraphs[0].IsBold));
        Assert.Equal("Senior Developer", paragraphs[1].Text);
        Assert.True(paragraphs[2].IsBullet);
        Assert.EndsWith("so that it wraps onto a second line here.", paragraphs[2].Text);
        Assert.StartsWith("**Frontend:** React", paragraphs[3].Text);
        Assert.Equal("**Backend:** C#, .NET", paragraphs[4].Text);
    }

    [Fact]
    public void PlainTextReader_ReadsMarkdownHeadingsAndBullets()
    {
        using var text = new MemoryStream(Encoding.UTF8.GetBytes("Alex Smith\n\n# Experience\n**Acme**\tJan 2020 – Present\n- Did a thing\n"));

        var paragraphs = new PlainTextReader().Read(text);

        Assert.Equal(["Alex Smith", "EXPERIENCE", "**Acme**", "Did a thing"], paragraphs.Select(p => p.Text));
        Assert.Equal("Jan 2020 – Present", paragraphs[2].RightText);
        Assert.True(paragraphs[3].IsBullet);
    }

    [Fact]
    public async Task Import_CreatesANewProfileFromAWordDocument()
    {
        using var database = new TestDatabase();
        var existing = await database.CreateProfile("Existing");
        const string body = """
            <w:p><w:r><w:t>Alex Smith</w:t></w:r></w:p>
            <w:p><w:r><w:t>alex@example.com</w:t></w:r></w:p>
            <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>EXPERIENCE</w:t></w:r></w:p>
            <w:p><w:r><w:rPr><w:b/></w:rPr><w:t>Acme</w:t></w:r><w:r><w:tab/><w:t>2020 – 2024</w:t></w:r></w:p>
            <w:p><w:r><w:t>Developer</w:t></w:r></w:p>
            <w:p><w:pPr><w:numPr/></w:pPr><w:r><w:t>Built things.</w:t></w:r></w:p>
            """;
        using var docx = Docx(body);

        var result = await database.Get<IImportFactory>().Import("Alex_Smith_CV.docx", docx);

        Assert.Equal(FactoryStatus.Success, result.Status);
        var profile = result.Value!;
        Assert.NotEqual(existing.Id, profile.Id);
        Assert.Equal(("Alex Smith CV", "Alex Smith", "alex@example.com"), (profile.Name, profile.FullName, profile.Email));
        var entry = profile.Sections.Single().Entries.Single();
        Assert.Equal(("Acme", "Developer", "2020 – 2024"), (entry.Heading, entry.Subheading, entry.DateRange));
        Assert.Equal(["Built things."], entry.Lines.Select(l => l.Text));
        Assert.Single(profile.Tags, t => t.IsAlwaysOn);
        Assert.Empty((await database.Get<IProfileFactory>().GetById(existing.Id)).Value!.Sections);
    }

    [Fact]
    public async Task Import_RejectsUnsupportedAndEmptyFiles()
    {
        using var database = new TestDatabase();
        var factory = database.Get<IImportFactory>();

        Assert.Equal(FactoryStatus.Invalid, (await factory.Import("cv.pages", new MemoryStream([1, 2, 3]))).Status);
        Assert.Equal(FactoryStatus.Invalid, (await factory.Import("cv.txt", new MemoryStream())).Status);
    }

    private static MemoryStream Docx(string body)
    {
        var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            using var writer = new StreamWriter(zip.CreateEntry("word/document.xml").Open());
            writer.Write($"""<?xml version="1.0" encoding="UTF-8"?><w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:body>{body}</w:body></w:document>""");
        }
        stream.Position = 0;
        return stream;
    }
}
