using System.Reflection;
using SmartCv.Host;

namespace SmartCv.Services;

public interface ITemplateFileService
{
    IReadOnlyList<string> BuiltInNames { get; }
    int EnsureBuiltIns();
}

public class TemplateFileService : ITemplateFileService
{
    private const string ResourcePrefix = "SmartCv.Templates.";
    private const string Extension = ".css";

    private readonly IAppPaths _paths;
    private readonly Assembly _assembly = typeof(TemplateFileService).Assembly;

    public TemplateFileService(IAppPaths paths)
    {
        _paths = paths;
        BuiltInNames = _assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix) && name.EndsWith(Extension))
            .Select(name => name[ResourcePrefix.Length..^Extension.Length])
            .Order()
            .ToList();
    }

    public IReadOnlyList<string> BuiltInNames { get; }

    public int EnsureBuiltIns()
    {
        Directory.CreateDirectory(_paths.Templates);
        var copied = 0;

        foreach (var name in BuiltInNames)
        {
            var target = Path.Combine(_paths.Templates, name + Extension);
            if (File.Exists(target)) continue;

            using var source = _assembly.GetManifestResourceStream(ResourcePrefix + name + Extension)!;
            using var file = File.Create(target);
            source.CopyTo(file);
            copied++;
        }

        return copied;
    }
}
