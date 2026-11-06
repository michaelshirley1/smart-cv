namespace SmartCv.Models;

public class TemplateSummary
{
    public const string DefaultName = "default";

    public string Name { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
}

public class Template : TemplateSummary
{
    public Dictionary<string, string> Values { get; set; } = [];
}
