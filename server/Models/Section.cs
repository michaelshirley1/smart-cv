namespace SmartCv.Models;

public enum SectionKind
{
    Summary,
    Experience,
    Education,
    Projects,
    Skills,
    Custom
}

public class Section
{
    public int Id { get; set; }
    public int ProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public SectionKind Kind { get; set; }
    public int SortOrder { get; set; }
    public int MaxLines { get; set; } = 5;
    public int MaxTotal { get; set; }
    public bool IsHidden { get; set; }
    public int Priority { get; set; } = Priorities.Normal;
    public List<int> TagIds { get; set; } = [];
    public List<Entry> Entries { get; set; } = [];
}
