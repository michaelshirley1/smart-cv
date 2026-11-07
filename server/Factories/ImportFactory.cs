using SmartCv.Models;
using SmartCv.Services;

namespace SmartCv.Factories;

public interface IImportFactory
{
    Task<FactoryResult<ProfileDetail>> Import(string fileName, Stream content);
}

public class ImportFactory : IImportFactory
{
    private const int MaxNameLength = 100;
    private const int MinMaxLines = 5;
    private const int MaxMaxLines = 50;

    private readonly IEnumerable<IDocumentReader> _readers;
    private readonly IProfileFactory _profileFactory;

    public ImportFactory(
        IEnumerable<IDocumentReader> readers,
        IProfileFactory profileFactory
    )
    {
        _readers = readers;
        _profileFactory = profileFactory;
    }

    public async Task<FactoryResult<ProfileDetail>> Import(string fileName, Stream content)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var reader = _readers.FirstOrDefault(candidate => candidate.Extensions.Contains(extension));
        if (reader is null) return FactoryResult<ProfileDetail>.Invalid("Choose a Word (.docx), PDF, .txt or .md file.");

        IReadOnlyList<ImportedParagraph> paragraphs;
        try
        {
            paragraphs = reader.Read(content);
        }
        catch (ImportException ex)
        {
            return FactoryResult<ProfileDetail>.Invalid(ex.Message);
        }

        if (paragraphs.Count == 0)
            return FactoryResult<ProfileDetail>.Invalid("No text was found in that file. Scanned PDFs, which are pictures of text, can't be read.");

        var cv = CvStructurer.Structure(paragraphs);
        var created = await _profileFactory.Create(new ProfileRequest
        {
            Name = ProfileName(fileName),
            FullName = cv.FullName,
            Email = cv.Email,
            Phone = cv.Phone,
            Location = cv.Location,
            Links = cv.Links,
        });
        if (created.Value is null) return created;

        var profile = created.Value;
        profile.Sections = cv.Sections.Select(ToSection).ToList();
        return await _profileFactory.Restore(profile.Id, profile);
    }

    private static Section ToSection(ImportedSection section) => new()
    {
        Title = section.Title,
        Kind = section.Kind,
        MaxLines = Math.Clamp(section.Entries.Select(entry => entry.Lines.Count).DefaultIfEmpty(0).Max(), MinMaxLines, MaxMaxLines),
        Entries = section.Entries.Select(entry => new Entry
        {
            Heading = entry.Heading,
            Subheading = entry.Subheading,
            Location = entry.Location,
            DateRange = entry.DateRange,
            Lines = entry.Lines.Select(text => new Line { Text = text }).ToList(),
        }).ToList(),
    };

    private static string ProfileName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName).Replace('_', ' ').Trim();
        if (name.Length == 0) name = "Imported CV";
        return name.Length > MaxNameLength ? name[..MaxNameLength] : name;
    }
}
