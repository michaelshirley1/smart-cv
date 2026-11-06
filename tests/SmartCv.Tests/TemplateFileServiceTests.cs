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
    public void EnsureBuiltIns_WritesMissingAndRestoresChangedFiles()
    {
        var service = new TemplateFileService(_paths);

        var firstRun = service.EnsureBuiltIns();
        var unchangedRun = service.EnsureBuiltIns();
        File.WriteAllText(Path.Combine(_paths.Templates, "default.css"), ".cv-wrapper { }");
        File.WriteAllText(Path.Combine(_paths.Templates, "mine.css"), ".cv-wrapper { }");
        var restoreRun = service.EnsureBuiltIns();

        Assert.Equal(3, firstRun);
        Assert.Equal(0, unchangedRun);
        Assert.Equal(1, restoreRun);
        Assert.Contains("--cv-name-size", File.ReadAllText(Path.Combine(_paths.Templates, "default.css")));
        Assert.Equal(".cv-wrapper { }", File.ReadAllText(Path.Combine(_paths.Templates, "mine.css")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_paths.Root)) Directory.Delete(_paths.Root, true);
    }
}
