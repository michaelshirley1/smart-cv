namespace SmartCv.Models;

public class Entry
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string Subheading { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string DateRange { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<Line> Lines { get; set; } = [];
}
