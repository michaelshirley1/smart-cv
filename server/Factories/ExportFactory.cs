using SmartCv.Host;
using SmartCv.Models;
using SmartCv.Services;

namespace SmartCv.Factories;

public interface IExportFactory
{
    Task<FactoryResult<ExportResult>> ExportPdfAsync(ExportRequest request, CancellationToken cancellationToken);
    FactoryResult<PrintJob> GetPrintJob(string id);
}

public class ExportFactory : IExportFactory
{
    private readonly IProfileFactory _profileFactory;
    private readonly ITemplateFileService _templateFiles;
    private readonly IPrintJobStore _printJobs;
    private readonly IPdfService _pdf;
    private readonly IFileDialogService _dialogs;
    private readonly IClientAddress _client;

    public ExportFactory(
        IProfileFactory profileFactory,
        ITemplateFileService templateFiles,
        IPrintJobStore printJobs,
        IPdfService pdf,
        IFileDialogService dialogs,
        IClientAddress client)
    {
        _profileFactory = profileFactory;
        _templateFiles = templateFiles;
        _printJobs = printJobs;
        _pdf = pdf;
        _dialogs = dialogs;
        _client = client;
    }

    public async Task<FactoryResult<ExportResult>> ExportPdfAsync(ExportRequest request, CancellationToken cancellationToken)
    {
        var profileResult = _profileFactory.GetById(request.ProfileId);
        if (profileResult.Value is null) return FactoryResult<ExportResult>.NotFound(profileResult.Error);

        var profile = profileResult.Value;
        var path = await _dialogs.AskSavePathAsync(FileName(profile));
        if (path is null) return FactoryResult<ExportResult>.Success(new ExportResult { Saved = false });

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

        return FactoryResult<ExportResult>.Success(new ExportResult { Saved = true, Path = path });
    }

    public FactoryResult<PrintJob> GetPrintJob(string id)
    {
        var job = _printJobs.Get(id);
        return job is null
            ? FactoryResult<PrintJob>.NotFound("That export has expired.")
            : FactoryResult<PrintJob>.Success(job);
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

    private static string FileName(ProfileDetail profile)
    {
        var name = string.IsNullOrWhiteSpace(profile.FullName) ? profile.Name : profile.FullName;
        var safe = string.Concat(name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? ' ' : character)).Trim();
        return $"{(safe.Length > 0 ? safe : "My")} CV.pdf";
    }
}
