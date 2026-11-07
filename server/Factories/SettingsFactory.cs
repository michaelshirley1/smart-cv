using SmartCv.Models;
using SmartCv.Repositories;
using SmartCv.Services;

namespace SmartCv.Factories;

public interface ISettingsFactory
{
    Task<AppSettings> Get();
    Task<AppSettings> Update(AppSettings settings);
    Task<OllamaStatus> GetOllamaStatus(string? url, CancellationToken cancellationToken);
}

public class SettingsFactory : ISettingsFactory
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IOllamaService _ollama;

    public SettingsFactory(
        ISettingsRepository settingsRepository,
        IOllamaService ollama
    )
    {
        _settingsRepository = settingsRepository;
        _ollama = ollama;
    }

    public async Task<AppSettings> Get()
    {
        var values = (await _settingsRepository.GetAll()).ToDictionary(s => s.Key, s => s.Value);

        return new AppSettings
        {
            MatchingMode = Enum.TryParse<MatchingMode>(values.GetValueOrDefault(SettingKeys.MatchingMode), true, out var mode)
                ? mode
                : MatchingMode.Keyword,
            OllamaUrl = string.IsNullOrWhiteSpace(values.GetValueOrDefault(SettingKeys.OllamaUrl))
                ? AppSettings.DefaultOllamaUrl
                : values[SettingKeys.OllamaUrl],
            OllamaModel = values.GetValueOrDefault(SettingKeys.OllamaModel) ?? string.Empty,
            LastProfileId = int.TryParse(values.GetValueOrDefault(SettingKeys.LastProfileId), out var profileId)
                ? profileId
                : null,
        };
    }

    public async Task<AppSettings> Update(AppSettings settings)
    {
        await _settingsRepository.Save(
        [
            new Setting { Key = SettingKeys.MatchingMode, Value = settings.MatchingMode.ToString().ToLowerInvariant() },
            new Setting { Key = SettingKeys.OllamaUrl, Value = settings.OllamaUrl.Trim() },
            new Setting { Key = SettingKeys.OllamaModel, Value = settings.OllamaModel.Trim() },
            new Setting { Key = SettingKeys.LastProfileId, Value = settings.LastProfileId?.ToString() ?? string.Empty },
        ]);

        return await Get();
    }

    public async Task<OllamaStatus> GetOllamaStatus(string? url, CancellationToken cancellationToken)
    {
        var target = string.IsNullOrWhiteSpace(url) ? (await Get()).OllamaUrl : url.Trim();
        return Uri.TryCreate(target, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? await _ollama.GetStatusAsync(target, cancellationToken)
            : new OllamaStatus { Reachable = false, Error = $"{target} isn't a valid web address." };
    }
}
