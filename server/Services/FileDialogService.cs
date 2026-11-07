using Photino.NET;

namespace SmartCv.Services;

public interface IFileDialogService
{
    bool CanBrowse { get; }
    void Attach(PhotinoWindow window);
    Task<string?> AskFolderAsync(string? initialFolder);
}

public class FileDialogService : IFileDialogService
{
    private PhotinoWindow? _window;

    public bool CanBrowse => _window is not null;

    public void Attach(PhotinoWindow window) => _window = window;

    public async Task<string?> AskFolderAsync(string? initialFolder)
    {
        if (_window is null) return null;

        var start = !string.IsNullOrWhiteSpace(initialFolder) && Directory.Exists(initialFolder) ? initialFolder : null;
        var chosen = await _window.ShowOpenFolderAsync("Choose where to save the PDF", start, false);
        var folder = chosen?.FirstOrDefault();
        return string.IsNullOrWhiteSpace(folder) ? null : folder;
    }
}
