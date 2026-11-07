using System.ComponentModel.DataAnnotations;

namespace SmartCv.Models;

public enum SectionDecision
{
    Untagged,
    Matched,
    AlwaysOn,
    Unmatched,
    Hidden
}

public enum LineDecision
{
    AlwaysOn,
    Matched,
    Untagged,
    Fallback,
    Unmatched,
    OverLimit,
    Hidden,
    SectionExcluded
}

public class TailorRequest
{
    [Range(1, int.MaxValue)]
    public int ProfileId { get; set; }

    [Required]
    [StringLength(50000, MinimumLength = 1)]
    public string JobDescription { get; set; } = string.Empty;
}

public class TagMatch
{
    public int TagId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<string> Terms { get; set; } = [];
}

public class SectionResult
{
    public int SectionId { get; set; }
    public bool Included { get; set; }
    public SectionDecision Decision { get; set; }
}

public class LineResult
{
    public int LineId { get; set; }
    public int EntryId { get; set; }
    public int SectionId { get; set; }
    public int Score { get; set; }
    public bool Included { get; set; }
    public LineDecision Decision { get; set; }
}

public class TailorResult
{
    public MatchingMode Mode { get; set; } = MatchingMode.Keyword;
    public string? Model { get; set; }
    public string? Notice { get; set; }
    public List<TagMatch> MatchedTags { get; set; } = [];
    public List<SectionResult> Sections { get; set; } = [];
    public List<LineResult> Lines { get; set; } = [];
    public List<int> TrimOrder { get; set; } = [];
}
