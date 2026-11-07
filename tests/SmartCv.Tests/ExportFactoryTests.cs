using Microsoft.Extensions.DependencyInjection;
using SmartCv.Factories;
using SmartCv.Host;
using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Tests;

public class ExportFactoryTests
{
    private sealed class FakeClientAddress : IClientAddress
    {
        public string BaseUrl => "http://127.0.0.1:1";
    }

    private static TestDatabase CreateDatabase() =>
        new(services => services.AddSingleton<IClientAddress>(new FakeClientAddress()));

    [Fact]
    public async Task Location_SuggestsAFileNameFromTheProfile()
    {
        using var database = CreateDatabase();
        var profile = await database.CreateProfile();
        await database.Get<IProfileFactory>().Update(profile.Id, new ProfileRequest { Name = "Backend: roles", FullName = "Alex Smith" });

        var location = (await database.Get<IExportFactory>().GetLocation(profile.Id)).Value!;

        Assert.Equal("Backend  roles.pdf", location.FileName);
        Assert.True(Directory.Exists(location.Folder));
        Assert.False(location.CanBrowse);
        Assert.Equal(FactoryStatus.NotFound, (await database.Get<IExportFactory>().GetLocation(99)).Status);
    }

    [Fact]
    public async Task Location_RemembersTheLastFolder()
    {
        using var database = CreateDatabase();
        var profile = await database.CreateProfile();
        var folder = Directory.CreateDirectory(Path.Combine(database.Paths.Root, "cvs")).FullName;
        await database.Get<ISettingsRepository>().Save([new Setting { Key = SettingKeys.ExportFolder, Value = folder }]);

        var location = (await database.Get<IExportFactory>().GetLocation(profile.Id)).Value!;

        Assert.Equal(folder, location.Folder);
    }

    [Fact]
    public async Task Export_RejectsAMissingFolder()
    {
        using var database = CreateDatabase();
        var profile = await database.CreateProfile();

        var result = await database.Get<IExportFactory>().ExportPdf(
            new ExportRequest { ProfileId = profile.Id, Folder = Path.Combine(database.Paths.Root, "missing") },
            CancellationToken.None);

        Assert.Equal(FactoryStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Export_AsksBeforeReplacingAFile()
    {
        using var database = CreateDatabase();
        var profile = await database.CreateProfile();
        var folder = database.Paths.Root;
        await File.WriteAllTextAsync(Path.Combine(folder, "Mine.pdf"), "old");

        var result = (await database.Get<IExportFactory>().ExportPdf(
            new ExportRequest { ProfileId = profile.Id, Folder = folder, FileName = "Mine" },
            CancellationToken.None)).Value!;

        Assert.False(result.Saved);
        Assert.True(result.Exists);
        Assert.Equal(Path.Combine(folder, "Mine.pdf"), result.Path);
        Assert.Equal("old", await File.ReadAllTextAsync(Path.Combine(folder, "Mine.pdf")));
    }

    [Fact]
    public async Task Browse_NeedsTheAppWindow()
    {
        using var database = CreateDatabase();

        var result = await database.Get<IExportFactory>().Browse(new BrowseRequest());

        Assert.Equal(FactoryStatus.Invalid, result.Status);
    }
}
