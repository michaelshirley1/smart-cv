using System.ComponentModel.DataAnnotations;

namespace SmartCv.Models;

public enum MatchingMode
{
    Keyword,
    Ollama
}

public static class SettingKeys
{
    public const string MatchingMode = "MatchingMode";
    public const string OllamaUrl = "OllamaUrl";
    public const string OllamaModel = "OllamaModel";
    public const string LastProfileId = "LastProfileId";
    public const string ExportFolder = "ExportFolder";
}

public class AppSettings
{
    public const string DefaultOllamaUrl = "http://127.0.0.1:11434";

    [Required]
    public MatchingMode MatchingMode { get; set; } = MatchingMode.Keyword;

    [Required]
    [Url]
    public string OllamaUrl { get; set; } = DefaultOllamaUrl;

    [MaxLength(200)]
    public string OllamaModel { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int? LastProfileId { get; set; }
}

public class AppInfo
{
    public string Version { get; set; } = string.Empty;
    public string DataFolder { get; set; } = string.Empty;
}
