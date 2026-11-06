using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SmartCv.Host;

namespace SmartCv.Services;

public interface ITemplateFileService
{
    IReadOnlyList<string> BuiltInNames { get; }
    int EnsureBuiltIns();
    IReadOnlyList<string> List();
    bool IsValidName(string name);
    bool Exists(string name);
    bool IsBuiltIn(string name);
    Dictionary<string, string>? Read(string name);
    Dictionary<string, string> ReadBuiltIn(string name);
    void Write(string name, IReadOnlyDictionary<string, string> values);
    bool Delete(string name);
}

public partial class TemplateFileService : ITemplateFileService
{
    private const string ResourcePrefix = "SmartCv.Templates.";
    private const string Extension = ".css";
    private const string PropertyPrefix = "--cv-";

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
            var target = PathFor(name);
            using var reader = new StreamReader(OpenBuiltIn(name));
            var content = reader.ReadToEnd();
            if (File.Exists(target) && File.ReadAllText(target) == content) continue;

            File.WriteAllText(target, content);
            copied++;
        }

        return copied;
    }

    public IReadOnlyList<string> List() =>
        Directory.EnumerateFiles(_paths.Templates, "*" + Extension)
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .Order()
            .ToList();

    public bool IsValidName(string name) => NamePattern().IsMatch(name);

    public bool Exists(string name) => IsValidName(name) && File.Exists(PathFor(name));

    public bool IsBuiltIn(string name) => BuiltInNames.Contains(name);

    public Dictionary<string, string>? Read(string name) =>
        Exists(name) ? Parse(File.ReadAllText(PathFor(name))) : null;

    public Dictionary<string, string> ReadBuiltIn(string name)
    {
        using var reader = new StreamReader(OpenBuiltIn(name));
        return Parse(reader.ReadToEnd());
    }

    public void Write(string name, IReadOnlyDictionary<string, string> values)
    {
        var css = new StringBuilder();
        css.AppendLine(".cv-wrapper {");
        foreach (var (key, value) in values)
        {
            css.AppendLine($"    {PropertyPrefix}{key}: {value};");
        }
        css.AppendLine("}");

        File.WriteAllText(PathFor(name), css.ToString());
    }

    public bool Delete(string name)
    {
        if (!Exists(name)) return false;
        File.Delete(PathFor(name));
        return true;
    }

    private string PathFor(string name) =>
        NamePattern().IsMatch(name)
            ? Path.Combine(_paths.Templates, name + Extension)
            : throw new ArgumentException($"Invalid template name {name}.", nameof(name));

    private Stream OpenBuiltIn(string name) =>
        _assembly.GetManifestResourceStream(ResourcePrefix + name + Extension)
            ?? throw new FileNotFoundException($"Built-in template {name} was not found.");

    private static Dictionary<string, string> Parse(string css) =>
        PropertyPattern().Matches(css)
            .GroupBy(match => match.Groups["key"].Value)
            .ToDictionary(group => group.Key, group => group.Last().Groups["value"].Value.Trim());

    [GeneratedRegex(@"--cv-(?<key>[a-z0-9-]+)\s*:\s*(?<value>[^;]+);")]
    private static partial Regex PropertyPattern();

    [GeneratedRegex("^[a-z0-9][a-z0-9-]{0,39}$")]
    private static partial Regex NamePattern();
}
