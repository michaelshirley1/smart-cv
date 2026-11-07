using SmartCv.Host;
using SmartCv.Models;
using SmartCv.Repositories;
using SmartCv.Services;

namespace SmartCv.Factories;

public interface IExportFactory
{
    Task<FactoryResult<ExportLocation>> GetLocation(int profileId);
    Task<FactoryResult<BrowseResult>> Browse(BrowseRequest request);
    Task<FactoryResult<ExportResult>> ExportPdf(ExportRequest request, CancellationToken cancellationToken);
    Task<FactoryResult<PrintJob>> GetPrintJob(string id);
}

public class ExportFactory : IExportFactory
{
    private readonly IProfileFactory _profileFactory;
    private readonly ITemplateFileService _templateFiles;
    private readonly IPrintJobStore _printJobs;
    private readonly IPdfService _pdf;
    private readonly IFileDialogService _dialogs;
    private readonly IClientAddress _client;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IAppPaths _paths;

    public ExportFactory(
        IProfileFactory profileFactory,
        ITemplateFileService templateFiles,
        IPrintJobStore printJobs,
        IPdfService pdf,
        IFileDialogService dialogs,
        IClientAddress client,
        ISettingsRepository settingsRepository,
        IAppPaths paths
    )
    {
        _profileFactory = profileFactory;
        _templateFiles = templateFiles;
        _printJobs = printJobs;
        _pdf = pdf;
        _dialogs = dialogs;
        _client = client;
        _settingsRepository = settingsRepository;
        _paths = paths;
    }

    public async Task<FactoryResult<ExportLocation>> GetLocation(int profileId)
    {
        var profileResult = await _profileFactory.GetById(profileId);
        if (profileResult.Value is null) return FactoryResult<ExportLocation>.NotFound(profileResult.Error);

        return FactoryResult<ExportLocation>.Success(new ExportLocation
        {
            Folder = await DefaultFolder(),
            FileName = FileName(profileResult.Value),
            CanBrowse = _dialogs.CanBrowse,
        });
    }

    public async Task<FactoryResult<BrowseResult>> Browse(BrowseRequest request)
    {
        if (!_dialogs.CanBrowse) return FactoryResult<BrowseResult>.Invalid("Choosing a folder needs the app window. Type the folder instead.");

        var folder = await _dialogs.AskFolderAsync(request.Folder?.Trim());
        return FactoryResult<BrowseResult>.Success(new BrowseResult { Folder = folder });
    }

    public async Task<FactoryResult<ExportResult>> ExportPdf(ExportRequest request, CancellationToken cancellationToken)
    {
        var profileResult = await _profileFactory.GetById(request.ProfileId);
        if (profileResult.Value is null) return FactoryResult<ExportResult>.NotFound(profileResult.Error);

        var profile = profileResult.Value;
        string path;
        if (string.IsNullOrWhiteSpace(request.Folder))
        {
            Directory.CreateDirectory(_paths.Exports);
            path = UniquePath(Path.Combine(_paths.Exports, FileName(profile)));
        }
        else
        {
            var folder = request.Folder.Trim();
            if (!Path.IsPathFullyQualified(folder) || !Directory.Exists(folder))
                return FactoryResult<ExportResult>.Invalid($"The folder {folder} doesn't exist.");

            var fileName = SafeFileName(string.IsNullOrWhiteSpace(request.FileName) ? FileName(profile) : request.FileName);
            path = Path.Combine(folder, fileName);
            if (File.Exists(path) && !request.Overwrite)
                return FactoryResult<ExportResult>.Success(new ExportResult { Saved = false, Exists = true, Path = path });
        }

        var job = _printJobs.Add(new PrintJob { Profile = profile, SectionIds = request.SectionIds, LineIds = request.LineIds });
        try
        {
            var pdf = await _pdf.RenderAsync($"{_client.BaseUrl}/print/{job.Id}", PageSetupFor(profile.TemplateName), cancellationToken);
            await File.WriteAllBytesAsync(path, pdf, cancellationToken);
        }
        catch (PdfException ex)
        {
            return FactoryResult<ExportResult>.Invalid(ex.Message);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return FactoryResult<ExportResult>.Invalid($"The PDF could not be saved to {path}.");
        }
        finally
        {
            _printJobs.Remove(job.Id);
        }

        if (!string.IsNullOrWhiteSpace(request.Folder))
            await _settingsRepository.Save([new Setting { Key = SettingKeys.ExportFolder, Value = Path.GetDirectoryName(path)! }]);

        return FactoryResult<ExportResult>.Success(new ExportResult { Saved = true, Path = path });
    }

    public Task<FactoryResult<PrintJob>> GetPrintJob(string id)
    {
        var job = _printJobs.Get(id);
        return Task.FromResult(job is null
            ? FactoryResult<PrintJob>.NotFound("That export has expired.")
            : FactoryResult<PrintJob>.Success(job));
    }

    private PageSetup PageSetupFor(string templateName)
    {
        var values = _templateFiles.Read(templateName) ?? _templateFiles.ReadBuiltIn(TemplateSummary.DefaultName);
        var defaults = new PageSetup();
        string Value(string key, string fallback) => values.TryGetValue(key, out var value) && value.Length > 0 ? value : fallback;

        return new PageSetup
        {
            Width = Value("page-width", defaults.Width),
            Height = Value("page-height", defaults.Height),
            MarginTop = Value("page-margin-top", defaults.MarginTop),
            MarginRight = Value("page-margin-right", defaults.MarginRight),
            MarginBottom = Value("page-margin-bottom", defaults.MarginBottom),
            MarginLeft = Value("page-margin-left", defaults.MarginLeft),
        };
    }

    private async Task<string> DefaultFolder()
    {
        var saved = (await _settingsRepository.GetAll()).FirstOrDefault(setting => setting.Key == SettingKeys.ExportFolder)?.Value;
        if (!string.IsNullOrWhiteSpace(saved) && Directory.Exists(saved)) return saved;

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return !string.IsNullOrWhiteSpace(documents) && Directory.Exists(documents) ? documents : _paths.Exports;
    }

    private static string FileName(ProfileDetail profile) => SafeFileName(profile.Name);

    private static string SafeFileName(string name)
    {
        var safe = string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? ' ' : character)).Trim().TrimEnd('.');
        if (safe.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) safe = safe[..^4].TrimEnd();
        return $"{(safe.Length > 0 ? safe : "My CV")}.pdf";
    }

    private static string UniquePath(string path)
    {
        var folder = Path.GetDirectoryName(path)!;
        var name = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);
        var candidate = path;
        for (var suffix = 2; File.Exists(candidate); suffix++) candidate = Path.Combine(folder, $"{name} ({suffix}){extension}");
        return candidate;
    }
}
