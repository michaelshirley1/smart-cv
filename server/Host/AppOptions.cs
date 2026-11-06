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

    public string ServerUrl => Mode == AppMode.Desktop ? DesktopServerUrl : DevServerUrl;

    public static AppOptions FromArgs(string[] args)
    {
        if (args.Contains("--headless")) return new AppOptions { Mode = AppMode.Headless };
        if (args.Contains("--dev")) return new AppOptions { Mode = AppMode.Dev };
        return new AppOptions();
    }
}
