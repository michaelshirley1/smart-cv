using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public class TailoringFactoryTests
{
    internal sealed class Cv
    {
        public required TestDatabase Database { get; init; }
        public required int ProfileId { get; init; }
        public required Dictionary<string, int> Tags { get; init; }
        public int AlwaysOn { get; init; }

        public async Task<ProfileDetail> Profile() => (await Database.Get<IProfileFactory>().GetById(ProfileId)).Value!;

        public async Task<int> AddSection(string title, SectionKind kind, int maxLines = 5, params string[] tags)
        {
            var section = (await Database.AddSection(ProfileId, title, kind)).Sections.Last();
            if (maxLines != 5) await Database.Get<ISectionFactory>().Update(section.Id, new SectionRequest { Title = title, Kind = kind, MaxLines = maxLines });
            if (tags.Length > 0) await Database.Get<ISectionFactory>().SetTags(section.Id, new TagLinksRequest { TagIds = tags.Select(TagId).ToList() });
            return section.Entries[0].Id;
        }

        public async Task<int> AddLine(int entryId, string text, params string[] tags) =>
            (await Database.AddLine(entryId, text, tags.Select(TagId).ToArray())).Sections.SelectMany(s => s.Entries).Single(e => e.Id == entryId).Lines.Last().Id;

        public int TagId(string name) => name == Tag.AlwaysOnName ? AlwaysOn : Tags[name];

        public async Task<TailorResult> Tailor(string jobDescription) =>
            (await Database.Get<ITailoringFactory>().Tailor(new TailorRequest { ProfileId = ProfileId, JobDescription = jobDescription }, CancellationToken.None)).Value!;
    }

    internal static async Task<Cv> CreateCv(TestDatabase database)
    {
        var profile = await database.CreateProfile();
        await database.AddTag(profile.Id, "Frontend", "react", "typescript");
        await database.AddTag(profile.Id, "Backend", "c#", ".net", "sql");
        profile = await database.AddTag(profile.Id, "Data", "kafka", "databricks");
        return new Cv
        {
            Database = database,
            ProfileId = profile.Id,
            Tags = profile.Tags.Where(t => !t.IsAlwaysOn).ToDictionary(t => t.Name, t => t.Id),
            AlwaysOn = profile.Tags.Single(t => t.IsAlwaysOn).Id,
        };
    }

    internal static LineResult Line(TailorResult result, int lineId) => result.Lines.Single(line => line.LineId == lineId);

    [Fact]
    public async Task MatchedTags_AreScoredAndSorted()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);

        var result = await cv.Tailor("Senior C# / .NET engineer. SQL Server. Some React.");

        Assert.Equal(["Backend", "Frontend"], result.MatchedTags.Select(tag => tag.Name));
        Assert.Equal(3, result.MatchedTags[0].Score);
        Assert.Equal(MatchingMode.Keyword, result.Mode);
    }

    [Fact]
    public async Task Lines_FollowTheRules()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience);
        var backend = await cv.AddLine(entry, "Rewrote the sync service", "Backend");
        var frontend = await cv.AddLine(entry, "Built a design system", "Frontend");
        var generic = await cv.AddLine(entry, "Mentored two juniors");
        var pinned = await cv.AddLine(entry, "Award winner", Tag.AlwaysOnName, "Frontend");
        var hidden = await cv.AddLine(entry, "Old line", "Backend");
        await database.Get<ILineFactory>().Update(hidden, new LineRequest { Text = "Old line", Override = LineOverride.Hidden });

        var result = await cv.Tailor("C# and SQL backend role");

        Assert.Equal(LineDecision.Matched, Line(result, backend).Decision);
        Assert.True(Line(result, backend).Included);
        Assert.Equal(LineDecision.Unmatched, Line(result, frontend).Decision);
        Assert.False(Line(result, frontend).Included);
        Assert.Equal(LineDecision.Untagged, Line(result, generic).Decision);
        Assert.True(Line(result, generic).Included);
        Assert.Equal(LineDecision.AlwaysOn, Line(result, pinned).Decision);
        Assert.True(Line(result, pinned).Included);
        Assert.Equal(LineDecision.Hidden, Line(result, hidden).Decision);
        Assert.False(Line(result, hidden).Included);
    }

    [Fact]
    public async Task MaxLines_KeepsTheBestMatchesFirst()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience, maxLines: 2);
        var generic = await cv.AddLine(entry, "Generic");
        var weak = await cv.AddLine(entry, "Frontend only", "Frontend");
        var strong = await cv.AddLine(entry, "Full stack", "Frontend", "Backend");
        var pinned = await cv.AddLine(entry, "Pinned", Tag.AlwaysOnName);

        var result = await cv.Tailor("React, C#");

        Assert.True(Line(result, pinned).Included);
        Assert.True(Line(result, strong).Included);
        Assert.Equal(LineDecision.OverLimit, Line(result, weak).Decision);
        Assert.Equal(LineDecision.OverLimit, Line(result, generic).Decision);
    }

    [Fact]
    public async Task Entries_KeepTheirBestLineAsAFallback()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience);
        var first = await cv.AddLine(entry, "Data pipeline", "Data");
        await cv.AddLine(entry, "Another data line", "Data");

        var result = await cv.Tailor("React developer");

        Assert.Equal(LineDecision.Fallback, Line(result, first).Decision);
        Assert.Single(result.Lines, line => line.Included);
    }

    [Fact]
    public async Task Sections_AreLeftOutWhenTheirTagsDontMatch()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var dataEntry = await cv.AddSection("Data Projects", SectionKind.Projects, tags: "Data");
        var dataLine = await cv.AddLine(dataEntry, "Built a Kafka pipeline");
        var summaryEntry = await cv.AddSection("Summary", SectionKind.Summary);
        await cv.AddLine(summaryEntry, "Engineer.");
        var pinnedEntry = await cv.AddSection("Languages", SectionKind.Custom, tags: [Tag.AlwaysOnName, "Data"]);
        await cv.AddLine(pinnedEntry, "English, Japanese");

        var result = await cv.Tailor("React developer");
        var profile = await cv.Profile();
        var decisions = result.Sections.ToDictionary(s => profile.Sections.Single(p => p.Id == s.SectionId).Title, s => s.Decision);

        Assert.Equal(SectionDecision.Unmatched, decisions["Data Projects"]);
        Assert.Equal(SectionDecision.Untagged, decisions["Summary"]);
        Assert.Equal(SectionDecision.AlwaysOn, decisions["Languages"]);
        Assert.Equal(LineDecision.SectionExcluded, Line(result, dataLine).Decision);
        Assert.False(Line(result, dataLine).Included);
    }

    [Fact]
    public async Task PinnedSections_KeepEveryLine()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Skills", SectionKind.Skills, maxLines: 1, tags: Tag.AlwaysOnName);
        var lines = new[] { await cv.AddLine(entry, "Frontend: React"), await cv.AddLine(entry, "Data: Kafka", "Data"), await cv.AddLine(entry, "Languages") };

        var result = await cv.Tailor("React developer");

        Assert.All(lines, id => Assert.Equal(LineDecision.AlwaysOn, Line(result, id).Decision));
        Assert.All(lines, id => Assert.True(Line(result, id).Included));
        Assert.Empty(result.TrimOrder);
    }

    [Fact]
    public async Task TrimOrder_DropsGenericLinesFirstAndNeverEmptiesAnEntry()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience);
        var strong = await cv.AddLine(entry, "Strong", "Frontend", "Backend");
        var weak = await cv.AddLine(entry, "Weak", "Frontend");
        var genericA = await cv.AddLine(entry, "Generic A");
        var genericB = await cv.AddLine(entry, "Generic B");
        var pinned = await cv.AddLine(entry, "Pinned", Tag.AlwaysOnName);
        var single = await cv.AddLine(await cv.AddSection("Education", SectionKind.Education), "Only line");

        var result = await cv.Tailor("React and C#");

        Assert.Equal([genericB, genericA, weak, strong], result.TrimOrder);
        Assert.DoesNotContain(pinned, result.TrimOrder);
        Assert.DoesNotContain(single, result.TrimOrder);
    }

    [Fact]
    public async Task MissingProfile_IsNotFound()
    {
        using var database = new TestDatabase();

        var result = await database.Get<ITailoringFactory>().Tailor(new TailorRequest { ProfileId = 99, JobDescription = "x" }, CancellationToken.None);

        Assert.Equal(FactoryStatus.NotFound, result.Status);
    }
}
