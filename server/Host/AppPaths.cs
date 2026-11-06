namespace SmartCv.Host;

public interface IAppPaths
{
    string Root { get; }
    string Database { get; }
    string Templates { get; }
    void EnsureCreated();
}

public class AppPaths : IAppPaths
{
    public AppPaths(string root)
    {
        Root = root;
        Database = Path.Combine(root, "smartcv.db");
        Templates = Path.Combine(root, "templates");
    }

    public string Root { get; }
    public string Database { get; }
    public string Templates { get; }

    public static AppPaths ForCurrentUser() =>
        new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartCv"));

    public void EnsureCreated()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(Templates);
    }
}
