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
}

public class AppSettings
{
    public const string DefaultOllamaUrl = "http://localhost:11434";

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
