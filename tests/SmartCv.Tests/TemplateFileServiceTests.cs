using SmartCv.Host;
using SmartCv.Services;

namespace SmartCv.Tests;

public sealed class TemplateFileServiceTests : IDisposable
{
    private readonly AppPaths _paths = new(Path.Combine(Path.GetTempPath(), "smartcv-tests", Guid.NewGuid().ToString("N")));

    [Fact]
    public void BuiltInNames_ListsEmbeddedTemplates()
    {
        var service = new TemplateFileService(_paths);

        Assert.Equal(["compact", "default", "modern"], service.BuiltInNames);
    }

    [Fact]
    public void EnsureBuiltIns_CopiesMissingFilesOnly()
    {
        var service = new TemplateFileService(_paths);

        var firstRun = service.EnsureBuiltIns();
        File.WriteAllText(Path.Combine(_paths.Templates, "default.css"), ".cv-wrapper { }");
        var secondRun = service.EnsureBuiltIns();

        Assert.Equal(3, firstRun);
        Assert.Equal(0, secondRun);
        Assert.Equal(".cv-wrapper { }", File.ReadAllText(Path.Combine(_paths.Templates, "default.css")));
        Assert.Contains("--cv-accent-colour", File.ReadAllText(Path.Combine(_paths.Templates, "modern.css")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_paths.Root)) Directory.Delete(_paths.Root, true);
    }
}
