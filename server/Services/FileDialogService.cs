using Photino.NET;
using SmartCv.Host;

namespace SmartCv.Services;

public interface IFileDialogService
{
    void Attach(PhotinoWindow window);
    Task<string?> AskSavePathAsync(string suggestedName);
}

public class FileDialogService : IFileDialogService
{
    private static readonly (string Name, string[] Extensions)[] PdfFilter = [("PDF document", ["pdf"])];

    private readonly IAppPaths _paths;
    private PhotinoWindow? _window;

    public FileDialogService(IAppPaths paths)
    {
        _paths = paths;
    }

    public void Attach(PhotinoWindow window) => _window = window;

    public async Task<string?> AskSavePathAsync(string suggestedName)
    {
        if (_window is null)
        {
            Directory.CreateDirectory(_paths.Exports);
            return UniquePath(Path.Combine(_paths.Exports, suggestedName));
        }

        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var chosen = await _window.ShowSaveFileAsync("Export PDF", Path.Combine(documents, suggestedName), PdfFilter);
        if (string.IsNullOrWhiteSpace(chosen)) return null;
        return chosen.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? chosen : chosen + ".pdf";
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
