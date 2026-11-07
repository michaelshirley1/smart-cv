namespace SmartCv.Models;

public enum LineOverride
{
    None,
    Pinned,
    Hidden
}

public class Line
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public LineOverride Override { get; set; }
    public int Priority { get; set; } = Priorities.Normal;
    public List<int> TagIds { get; set; } = [];
}
