using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public class SettingsFactoryTests
{
    private static ISettingsFactory CreateFactory(TestDatabase database) => database.Get<ISettingsFactory>();

    [Fact]
    public async Task Get_ReturnsSeededDefaults()
    {
        using var database = new TestDatabase();

        var settings = await CreateFactory(database).Get();

        Assert.Equal(MatchingMode.Keyword, settings.MatchingMode);
        Assert.Equal(AppSettings.DefaultOllamaUrl, settings.OllamaUrl);
        Assert.Equal(string.Empty, settings.OllamaModel);
        Assert.Null(settings.LastProfileId);
    }

    [Fact]
    public async Task Update_PersistsValues()
    {
        using var database = new TestDatabase();
        var factory = CreateFactory(database);

        await factory.Update(new AppSettings
        {
            MatchingMode = MatchingMode.Ollama,
            OllamaUrl = " http://localhost:9999 ",
            OllamaModel = "llama3.2",
            LastProfileId = 3,
        });
        var settings = await CreateFactory(database).Get();

        Assert.Equal(MatchingMode.Ollama, settings.MatchingMode);
        Assert.Equal("http://localhost:9999", settings.OllamaUrl);
        Assert.Equal("llama3.2", settings.OllamaModel);
        Assert.Equal(3, settings.LastProfileId);
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("ftp://localhost:11434")]
    public async Task OllamaStatus_RejectsBadAddresses(string url)
    {
        using var database = new TestDatabase();

        var status = await CreateFactory(database).GetOllamaStatus(url, CancellationToken.None);

        Assert.False(status.Reachable);
        Assert.Contains("isn't a valid web address", status.Error);
    }
}
