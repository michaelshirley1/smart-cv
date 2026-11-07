using System.Text.RegularExpressions;
using SmartCv.Models;
using SmartCv.Repositories;
using SmartCv.Services;

namespace SmartCv.Factories;

public interface ITemplateFactory
{
    Task<IEnumerable<TemplateSummary>> GetAll();
    Task<FactoryResult<Template>> GetByName(string name);
    Task<FactoryResult<Template>> Create(TemplateRequest request);
    Task<FactoryResult<Template>> Update(string name, TemplateRequest request);
    Task<FactoryResult> Delete(string name);
}

public partial class TemplateFactory : ITemplateFactory
{
    private const int MaxNameLength = 40;
    private const int MaxValueLength = 200;
    private static readonly char[] ForbiddenValueCharacters = [';', '{', '}', '<', '>', '\r', '\n'];

    private readonly ITemplateFileService _templateFiles;
    private readonly IProfileRepository _profileRepository;

    public TemplateFactory(
        ITemplateFileService templateFiles,
        IProfileRepository profileRepository
    )
    {
        _templateFiles = templateFiles;
        _profileRepository = profileRepository;
    }

    public Task<IEnumerable<TemplateSummary>> GetAll() =>
        Task.FromResult(_templateFiles.List().Select(name => new TemplateSummary { Name = name, IsBuiltIn = _templateFiles.IsBuiltIn(name) }));

    public Task<FactoryResult<Template>> GetByName(string name)
    {
        var values = _templateFiles.Read(name);
        if (values is null) return Task.FromResult(FactoryResult<Template>.NotFound($"Template {name} not found."));

        return Task.FromResult(FactoryResult<Template>.Success(ToTemplate(name, values)));
    }

    public Task<FactoryResult<Template>> Create(TemplateRequest request)
    {
        var name = Slugify(request.Name);
        if (name.Length == 0) return Task.FromResult(FactoryResult<Template>.Invalid("Template names need at least one letter or number."));
        if (_templateFiles.Exists(name)) return Task.FromResult(FactoryResult<Template>.Invalid($"A template called {name} already exists."));

        var error = ValidateValues(request.Values);
        if (error is not null) return Task.FromResult(FactoryResult<Template>.Invalid(error));

        var values = Merge(_templateFiles.ReadBuiltIn(TemplateSummary.DefaultName), request.Values);
        _templateFiles.Write(name, values);

        return Task.FromResult(FactoryResult<Template>.Success(ToTemplate(name, values)));
    }

    public async Task<FactoryResult<Template>> Update(string name, TemplateRequest request)
    {
        var current = _templateFiles.Read(name);
        if (current is null) return FactoryResult<Template>.NotFound($"Template {name} not found.");
        if (_templateFiles.IsBuiltIn(name))
            return FactoryResult<Template>.Invalid("Built-in templates are read-only. Save your changes as a new template.");

        var newName = Slugify(request.Name);
        if (newName.Length == 0) return FactoryResult<Template>.Invalid("Template names need at least one letter or number.");
        if (newName != name && _templateFiles.Exists(newName))
            return FactoryResult<Template>.Invalid($"A template called {newName} already exists.");

        var error = ValidateValues(request.Values);
        if (error is not null) return FactoryResult<Template>.Invalid(error);

        var values = Merge(Merge(_templateFiles.ReadBuiltIn(TemplateSummary.DefaultName), current), request.Values);
        _templateFiles.Write(newName, values);

        if (newName != name)
        {
            _templateFiles.Delete(name);
            await _profileRepository.ReplaceTemplate(name, newName);
        }

        return FactoryResult<Template>.Success(ToTemplate(newName, values));
    }

    public async Task<FactoryResult> Delete(string name)
    {
        if (!_templateFiles.Exists(name)) return FactoryResult.NotFound($"Template {name} not found.");
        if (_templateFiles.IsBuiltIn(name)) return FactoryResult.Invalid("Built-in templates can't be deleted.");

        _templateFiles.Delete(name);
        await _profileRepository.ReplaceTemplate(name, TemplateSummary.DefaultName);

        return FactoryResult.Success();
    }

    private string? ValidateValues(IReadOnlyDictionary<string, string> values)
    {
        var allowedKeys = _templateFiles.ReadBuiltIn(TemplateSummary.DefaultName).Keys.ToHashSet();

        foreach (var (key, value) in values)
        {
            if (!allowedKeys.Contains(key)) return $"Unknown template setting {key}.";
            if (string.IsNullOrWhiteSpace(value)) return $"Template setting {key} needs a value.";
            if (value.Length > MaxValueLength) return $"Template setting {key} is too long.";
            if (value.IndexOfAny(ForbiddenValueCharacters) >= 0) return $"Template setting {key} contains characters that aren't allowed.";
        }

        return null;
    }

    private Template ToTemplate(string name, Dictionary<string, string> values) => new()
    {
        Name = name,
        IsBuiltIn = _templateFiles.IsBuiltIn(name),
        Values = values,
    };

    private static Dictionary<string, string> Merge(Dictionary<string, string> baseValues, IReadOnlyDictionary<string, string> overrides)
    {
        var merged = new Dictionary<string, string>(baseValues);
        foreach (var (key, value) in overrides)
        {
            merged[key] = value.Trim();
        }
        return merged;
    }

    private static string Slugify(string name)
    {
        var slug = NonSlugCharacters().Replace(name.Trim().ToLowerInvariant(), "-").Trim('-');
        return slug.Length > MaxNameLength ? slug[..MaxNameLength].TrimEnd('-') : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();
}
