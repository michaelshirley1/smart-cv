namespace SmartCv.Models;

public class Profile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public int TargetPages { get; set; } = 1;
    public string FullName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> Links { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProfileDetail : Profile
{
    public List<Tag> Tags { get; set; } = [];
    public List<Section> Sections { get; set; } = [];
}
