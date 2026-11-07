using SmartCv.Host;
using SmartCv.Services;

namespace SmartCv;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var options = AppOptions.FromArgs(args);
        var paths = options.CreatePaths();
        paths.EnsureCreated();
        var app = ServerSetup.Build(args, options, paths);

        AppInitialiser.Run(app.Services);

        if (options.Mode == AppMode.Headless)
        {
            app.Run();
            return;
        }

        app.StartAsync().GetAwaiter().GetResult();

        var url = options.Mode == AppMode.Dev ? AppOptions.DevClientUrl : app.Urls.First();
        var window = AppWindow.Create(url, paths);
        app.Services.GetRequiredService<IFileDialogService>().Attach(window);
        window.WaitForClose();

        app.StopAsync().GetAwaiter().GetResult();
    }
}
