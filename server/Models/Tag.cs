namespace SmartCv.Models;

public class Tag
{
    public const string AlwaysOnName = "Always on";

    public int Id { get; set; }
    public int ProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = [];
    public string Colour { get; set; } = string.Empty;
    public bool IsAlwaysOn { get; set; }
    public int UsageCount { get; set; }
}

public class TagLink
{
    public int OwnerId { get; set; }
    public int TagId { get; set; }
}
