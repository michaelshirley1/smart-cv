using Microsoft.Extensions.DependencyInjection;
using SmartCv.Factories;
using SmartCv.Models;
using SmartCv.Services;
using static SmartCv.Tests.TailoringFactoryTests;

namespace SmartCv.Tests;

public class OllamaTailoringTests
{
    private sealed class FakeOllama : IOllamaService
    {
        public Func<OllamaLine, int> Score { get; set; } = _ => 0;
        public Exception? Failure { get; set; }
        public List<List<int>> Calls { get; } = [];

        public Task<OllamaStatus> GetStatusAsync(string baseUrl, CancellationToken cancellationToken) =>
            Task.FromResult(new OllamaStatus { Reachable = true, Models = ["llama3.2"] });

        public Task<Dictionary<int, int>> ScoreAsync(string baseUrl, string model, string jobDescription, IReadOnlyList<OllamaLine> lines, CancellationToken cancellationToken)
        {
            Calls.Add(lines.Select(line => line.Id).ToList());
            if (Failure is not null) throw Failure;
            return Task.FromResult(lines.ToDictionary(line => line.Id, Score));
        }
    }

    private static async Task<(TestDatabase Database, FakeOllama Ollama)> CreateDatabase(string model = "llama3.2")
    {
        var ollama = new FakeOllama();
        var database = new TestDatabase(services => services.AddSingleton<IOllamaService>(ollama));
        await database.Get<ISettingsFactory>().Update(new AppSettings { MatchingMode = MatchingMode.Ollama, OllamaModel = model });
        return (database, ollama);
    }

    [Fact]
    public async Task ModelScores_DecideTheLines()
    {
        var (database, ollama) = await CreateDatabase();
        using var _ = database;
        ollama.Score = line => line.Text.Contains("React") ? 9 : line.Text.Contains("design") ? 6 : 2;
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience);
        var react = await cv.AddLine(entry, "Built **React** apps");
        var design = await cv.AddLine(entry, "Ran design reviews");
        var unrelated = await cv.AddLine(entry, "Managed the office plants");

        var result = await cv.Tailor("Frontend engineer");

        Assert.Equal(MatchingMode.Ollama, result.Mode);
        Assert.Equal("llama3.2", result.Model);
        Assert.Null(result.Notice);
        Assert.Equal((LineDecision.Matched, 9), (Line(result, react).Decision, Line(result, react).Score));
        Assert.Equal(LineDecision.Matched, Line(result, design).Decision);
        Assert.Equal(LineDecision.Unmatched, Line(result, unrelated).Decision);
        Assert.Equal([design], result.TrimOrder);
        Assert.DoesNotContain(ollama.Calls.Single(), id => id == 0);
    }

    [Fact]
    public async Task RelevantLines_KeepATaggedSectionThatKeywordsMissed()
    {
        var (database, ollama) = await CreateDatabase();
        using var _ = database;
        ollama.Score = line => line.Text.Contains("pipeline") ? 8 : 0;
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Data Projects", SectionKind.Projects, tags: "Data");
        var line = await cv.AddLine(entry, "Built a streaming pipeline");

        var result = await cv.Tailor("Event-driven systems engineer");

        Assert.Equal(SectionDecision.Matched, result.Sections.Single().Decision);
        Assert.True(Line(result, line).Included);
    }

    [Fact]
    public async Task PinnedAndHiddenLines_AreNotSentToTheModel()
    {
        var (database, ollama) = await CreateDatabase();
        using var _ = database;
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience);
        var normal = await cv.AddLine(entry, "Normal line");
        await cv.AddLine(entry, "Pinned line", Tag.AlwaysOnName);
        var hidden = await cv.AddLine(entry, "Hidden line");
        await database.Get<ILineFactory>().Update(hidden, new LineRequest { Text = "Hidden line", Override = LineOverride.Hidden });
        await cv.AddLine(await cv.AddSection("Skills", SectionKind.Skills, tags: Tag.AlwaysOnName), "Pinned section line");

        await cv.Tailor("Anything");

        Assert.Equal([normal], ollama.Calls.Single());
    }

    [Fact]
    public async Task Scores_AreCachedSoOnlyChangedLinesAreSent()
    {
        var (database, ollama) = await CreateDatabase();
        using var _ = database;
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience);
        var first = await cv.AddLine(entry, "First");
        var second = await cv.AddLine(entry, "Second");

        await cv.Tailor("Same advert");
        await database.Get<ILineFactory>().Update(second, new LineRequest { Text = "Second, edited" });
        await cv.Tailor("Same advert");
        await cv.Tailor("A different advert");

        Assert.Equal([first, second], ollama.Calls[0]);
        Assert.Equal([second], ollama.Calls[1]);
        Assert.Equal([first, second], ollama.Calls[2]);
    }

    [Fact]
    public async Task Failures_FallBackToKeywordsWithANotice()
    {
        var (database, ollama) = await CreateDatabase();
        using var _ = database;
        ollama.Failure = new OllamaException("Ollama isn't answering at http://localhost:11434.");
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience);
        var react = await cv.AddLine(entry, "React work", "Frontend");

        var result = await cv.Tailor("React developer");

        Assert.Equal(MatchingMode.Keyword, result.Mode);
        Assert.Equal("Ollama isn't answering at http://localhost:11434. Keyword matching was used instead.", result.Notice);
        Assert.Equal(LineDecision.Matched, Line(result, react).Decision);
    }

    [Fact]
    public async Task MissingModel_UsesKeywordsWithANotice()
    {
        var (database, ollama) = await CreateDatabase(model: "");
        using var _ = database;
        var cv = await CreateCv(database);
        await cv.AddLine(await cv.AddSection("Experience", SectionKind.Experience), "Line");

        var result = await cv.Tailor("Anything");

        Assert.Equal(MatchingMode.Keyword, result.Mode);
        Assert.Contains("No Ollama model", result.Notice);
        Assert.Empty(ollama.Calls);
    }
}
