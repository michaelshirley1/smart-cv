using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Factories;

public interface ISettingsFactory
{
    AppSettings Get();
    AppSettings Update(AppSettings settings);
}

public class SettingsFactory : ISettingsFactory
{
    private readonly ISettingsRepository _settingsRepository;

    public SettingsFactory(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public AppSettings Get()
    {
        var values = _settingsRepository.GetAll().ToDictionary(s => s.Key, s => s.Value);

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

    public AppSettings Update(AppSettings settings)
    {
        _settingsRepository.Save(
        [
            new Setting { Key = SettingKeys.MatchingMode, Value = settings.MatchingMode.ToString().ToLowerInvariant() },
            new Setting { Key = SettingKeys.OllamaUrl, Value = settings.OllamaUrl.Trim() },
            new Setting { Key = SettingKeys.OllamaModel, Value = settings.OllamaModel.Trim() },
            new Setting { Key = SettingKeys.LastProfileId, Value = settings.LastProfileId?.ToString() ?? string.Empty },
        ]);

        return Get();
    }
}
