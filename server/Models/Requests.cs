using System.ComponentModel.DataAnnotations;

namespace SmartCv.Models;

public class ProfileRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(40)]
    public string TemplateName { get; set; } = string.Empty;

    [Range(1, 5)]
    public int TargetPages { get; set; } = 1;

    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(200)]
    public string Headline { get; set; } = string.Empty;

    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(10)]
    public List<string> Links { get; set; } = [];
}

public class JobDescriptionRequest
{
    [StringLength(50000)]
    public string JobDescription { get; set; } = string.Empty;
}

public class SectionRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    public SectionKind Kind { get; set; } = SectionKind.Custom;

    [Range(1, 50)]
    public int MaxLines { get; set; } = 5;

    public bool IsHidden { get; set; }
}

public class CreateSectionRequest : SectionRequest
{
    [Range(1, int.MaxValue)]
    public int ProfileId { get; set; }
}

public class EntryRequest
{
    [StringLength(200)]
    public string Heading { get; set; } = string.Empty;

    [StringLength(200)]
    public string Subheading { get; set; } = string.Empty;

    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [StringLength(100)]
    public string DateRange { get; set; } = string.Empty;
}

public class CreateEntryRequest : EntryRequest
{
    [Range(1, int.MaxValue)]
    public int SectionId { get; set; }
}

public class LineRequest
{
    [StringLength(1000)]
    public string Text { get; set; } = string.Empty;

    public LineOverride Override { get; set; } = LineOverride.None;
}

public class CreateLineRequest : LineRequest
{
    [Range(1, int.MaxValue)]
    public int EntryId { get; set; }

    public List<int> TagIds { get; set; } = [];
}

public class TagRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public List<string> Keywords { get; set; } = [];

    [StringLength(20)]
    public string Colour { get; set; } = string.Empty;
}

public class CreateTagRequest : TagRequest
{
    [Range(1, int.MaxValue)]
    public int ProfileId { get; set; }
}

public class ReorderRequest
{
    [Range(1, int.MaxValue)]
    public int ParentId { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> Ids { get; set; } = [];
}

public class TagLinksRequest
{
    [Required]
    public List<int> TagIds { get; set; } = [];
}

public class TemplateRequest
{
    [Required]
    [StringLength(40, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Dictionary<string, string> Values { get; set; } = [];
}
