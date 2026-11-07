using PuppeteerSharp;
using PuppeteerSharp.Media;
using SmartCv.Host;
using SmartCv.Models;

namespace SmartCv.Services;

public interface IPdfService
{
    Task<byte[]> RenderAsync(string url, PageSetup setup, CancellationToken cancellationToken);
}

public class PdfService : IPdfService
{
    private const string ReadySelector = "[data-print-ready]";
    private static readonly TimeSpan ReadyTimeout = TimeSpan.FromSeconds(30);

    private readonly IAppPaths _paths;
    private readonly ILogger<PdfService> _logger;
    private readonly SemaphoreSlim _download = new(1, 1);

    public PdfService(IAppPaths paths, ILogger<PdfService> logger)
    {
        _paths = paths;
        _logger = logger;
    }

    public async Task<byte[]> RenderAsync(string url, PageSetup setup, CancellationToken cancellationToken)
    {
        var executable = await BrowserPathAsync();

        try
        {
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = executable,
                Args = ["--no-first-run", "--disable-extensions"],
            });
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);
            await page.WaitForSelectorAsync(ReadySelector, new WaitForSelectorOptions { Timeout = (int)ReadyTimeout.TotalMilliseconds });
            cancellationToken.ThrowIfCancellationRequested();

            return await page.PdfDataAsync(new PdfOptions
            {
                Width = setup.Width,
                Height = setup.Height,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = setup.MarginTop,
                    Right = setup.MarginRight,
                    Bottom = setup.MarginBottom,
                    Left = setup.MarginLeft,
                },
            });
        }
        catch (Exception ex) when (ex is PuppeteerException or TimeoutException or IOException)
        {
            _logger.LogError(ex, "Failed to render the PDF from {Url}", url);
            throw new PdfException("The PDF could not be created.", ex);
        }
    }

    private async Task<string> BrowserPathAsync()
    {
        var installed = InstalledBrowsers().FirstOrDefault(File.Exists);
        if (installed is not null) return installed;

        await _download.WaitAsync();
        try
        {
            var fetcher = new BrowserFetcher(new BrowserFetcherOptions { Path = _paths.Browsers, Browser = SupportedBrowser.Chrome });
            var downloaded = await fetcher.DownloadAsync();
            return downloaded.GetExecutablePath();
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or PuppeteerException)
        {
            throw new PdfException("No Chrome, Edge or Chromium was found, and one could not be downloaded.", ex);
        }
        finally
        {
            _download.Release();
        }
    }

    private static IEnumerable<string> InstalledBrowsers()
    {
        if (OperatingSystem.IsWindows())
        {
            var roots = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            };
            return roots.SelectMany(root => new[]
            {
                Path.Combine(root, "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(root, "Google", "Chrome", "Application", "chrome.exe"),
            });
        }

        if (OperatingSystem.IsMacOS())
        {
            return
            [
                "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
                "/Applications/Chromium.app/Contents/MacOS/Chromium",
                "/Applications/Brave Browser.app/Contents/MacOS/Brave Browser",
            ];
        }

        var path = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        return new[] { "google-chrome", "google-chrome-stable", "chromium", "chromium-browser", "microsoft-edge" }
            .SelectMany(name => path.Select(folder => Path.Combine(folder, name)));
    }
}
