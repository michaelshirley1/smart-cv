namespace SmartCv.Models;

public class ImportedParagraph
{
    public string Text { get; set; } = string.Empty;
    public string? RightText { get; set; }
    public bool IsBullet { get; set; }
    public bool IsBold { get; set; }
}

public class ImportedEntry
{
    public string Heading { get; set; } = string.Empty;
    public string Subheading { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public List<string> Lines { get; set; } = [];
}

public class ImportedSection
{
    public string Title { get; set; } = string.Empty;
    public SectionKind Kind { get; set; }
    public List<ImportedEntry> Entries { get; set; } = [];
}

public class ImportedCv
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> Links { get; set; } = [];
    public List<ImportedSection> Sections { get; set; } = [];
}

public class ImportException(string message, Exception? innerException = null) : Exception(message, innerException);
