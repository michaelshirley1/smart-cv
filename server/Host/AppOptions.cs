namespace SmartCv.Host;

public enum AppMode
{
    Desktop,
    Dev,
    Headless
}

public class AppOptions
{
    public const string DevServerUrl = "http://127.0.0.1:5080";
    public const string DevClientUrl = "http://localhost:5173";
    public const string DesktopServerUrl = "http://127.0.0.1:0";

    public AppMode Mode { get; init; } = AppMode.Desktop;
    public string? DataFolder { get; init; }

    public string ServerUrl => Mode == AppMode.Desktop ? DesktopServerUrl : DevServerUrl;

    public AppPaths CreatePaths() =>
        DataFolder is null ? AppPaths.ForCurrentUser() : new AppPaths(Path.GetFullPath(DataFolder));

    public static AppOptions FromArgs(string[] args)
    {
        var dataIndex = Array.IndexOf(args, "--data");
        var dataFolder = dataIndex >= 0 && dataIndex + 1 < args.Length ? args[dataIndex + 1] : null;

        var mode = args.Contains("--headless") ? AppMode.Headless
            : args.Contains("--dev") ? AppMode.Dev
            : AppMode.Desktop;

        return new AppOptions { Mode = mode, DataFolder = dataFolder };
    }
}
