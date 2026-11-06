using Photino.NET;

namespace SmartCv.Host;

public static class AppWindow
{
    public static void Show(string url)
    {
        var window = new PhotinoWindow()
            .SetTitle("Smart CV")
            .SetUseOsDefaultSize(false)
            .SetSize(1440, 900)
            .SetMinSize(960, 640)
            .Center()
            .SetResizable(true)
            .Load(new Uri(url));

        window.WaitForClose();
    }
}
