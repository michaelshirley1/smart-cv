using Photino.NET;

namespace SmartCv.Host;

public static class AppWindow
{
    public static PhotinoWindow Create(string url, IAppPaths paths)
    {
        return new PhotinoWindow()
            .SetTitle($"Smart CV {typeof(AppWindow).Assembly.GetName().Version?.ToString(3)}")
            .SetIconFile(IconFile(paths))
            .SetUseOsDefaultSize(false)
            .SetSize(1440, 900)
            .SetMinSize(960, 640)
            .Center()
            .SetResizable(true)
            .Load(new Uri(url));
    }

    private static string IconFile(IAppPaths paths)
    {
        var name = OperatingSystem.IsWindows() ? "app.ico" : "app.png";
        var target = Path.Combine(paths.Root, name);
        using var source = typeof(AppWindow).Assembly.GetManifestResourceStream($"SmartCv.Assets.{name}")!;
        using var file = File.Create(target);
        source.CopyTo(file);
        return target;
    }
}
