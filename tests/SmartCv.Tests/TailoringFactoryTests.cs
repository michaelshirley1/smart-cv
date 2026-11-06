using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public class TailoringFactoryTests
{
    private sealed class Cv
    {
        public required TestDatabase Database { get; init; }
        public required int ProfileId { get; init; }
        public required Dictionary<string, int> Tags { get; init; }
        public int AlwaysOn { get; init; }

        public ProfileDetail Profile => Database.Get<IProfileFactory>().GetById(ProfileId).Value!;

        public int AddSection(string title, SectionKind kind, int maxLines = 5, params string[] tags)
        {
            var section = Database.AddSection(ProfileId, title, kind).Sections.Last();
            if (maxLines != 5) Database.Get<ISectionFactory>().Update(section.Id, new SectionRequest { Title = title, Kind = kind, MaxLines = maxLines });
            if (tags.Length > 0) Database.Get<ISectionFactory>().SetTags(section.Id, new TagLinksRequest { TagIds = tags.Select(TagId).ToList() });
            return section.Entries[0].Id;
        }

        public int AddLine(int entryId, string text, params string[] tags) =>
            Database.AddLine(entryId, text, tags.Select(TagId).ToArray()).Sections.SelectMany(s => s.Entries).Single(e => e.Id == entryId).Lines.Last().Id;

        public int TagId(string name) => name == Tag.AlwaysOnName ? AlwaysOn : Tags[name];

        public TailorResult Tailor(string jobDescription) =>
            Database.Get<ITailoringFactory>().Tailor(new TailorRequest { ProfileId = ProfileId, JobDescription = jobDescription }).Value!;
    }

    private static Cv CreateCv(TestDatabase database)
    {
        var profile = database.CreateProfile();
        database.AddTag(profile.Id, "Frontend", "react", "typescript");
        database.AddTag(profile.Id, "Backend", "c#", ".net", "sql");
        profile = database.AddTag(profile.Id, "Data", "kafka", "databricks");
        return new Cv
        {
            Database = database,
            ProfileId = profile.Id,
            Tags = profile.Tags.Where(t => !t.IsAlwaysOn).ToDictionary(t => t.Name, t => t.Id),
            AlwaysOn = profile.Tags.Single(t => t.IsAlwaysOn).Id,
        };
    }

    private static LineResult Line(TailorResult result, int lineId) => result.Lines.Single(line => line.LineId == lineId);

    [Fact]
    public void MatchedTags_AreScoredAndSorted()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);

        var result = cv.Tailor("Senior C# / .NET engineer. SQL Server. Some React.");

        Assert.Equal(["Backend", "Frontend"], result.MatchedTags.Select(tag => tag.Name));
        Assert.Equal(3, result.MatchedTags[0].Score);
        Assert.Equal(MatchingMode.Keyword, result.Mode);
    }

    [Fact]
    public void Lines_FollowTheRules()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var entry = cv.AddSection("Experience", SectionKind.Experience);
        var backend = cv.AddLine(entry, "Rewrote the sync service", "Backend");
        var frontend = cv.AddLine(entry, "Built a design system", "Frontend");
        var generic = cv.AddLine(entry, "Mentored two juniors");
        var pinned = cv.AddLine(entry, "Award winner", Tag.AlwaysOnName, "Frontend");
        var hidden = cv.AddLine(entry, "Old line", "Backend");
        database.Get<ILineFactory>().Update(hidden, new LineRequest { Text = "Old line", Override = LineOverride.Hidden });

        var result = cv.Tailor("C# and SQL backend role");

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
    public void MaxLines_KeepsTheBestMatchesFirst()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var entry = cv.AddSection("Experience", SectionKind.Experience, maxLines: 2);
        var generic = cv.AddLine(entry, "Generic");
        var weak = cv.AddLine(entry, "Frontend only", "Frontend");
        var strong = cv.AddLine(entry, "Full stack", "Frontend", "Backend");
        var pinned = cv.AddLine(entry, "Pinned", Tag.AlwaysOnName);

        var result = cv.Tailor("React, C#");

        Assert.True(Line(result, pinned).Included);
        Assert.True(Line(result, strong).Included);
        Assert.Equal(LineDecision.OverLimit, Line(result, weak).Decision);
        Assert.Equal(LineDecision.OverLimit, Line(result, generic).Decision);
    }

    [Fact]
    public void Entries_KeepTheirBestLineAsAFallback()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var entry = cv.AddSection("Experience", SectionKind.Experience);
        var first = cv.AddLine(entry, "Data pipeline", "Data");
        cv.AddLine(entry, "Another data line", "Data");

        var result = cv.Tailor("React developer");

        Assert.Equal(LineDecision.Fallback, Line(result, first).Decision);
        Assert.Single(result.Lines, line => line.Included);
    }

    [Fact]
    public void Sections_AreLeftOutWhenTheirTagsDontMatch()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var dataEntry = cv.AddSection("Data Projects", SectionKind.Projects, tags: "Data");
        var dataLine = cv.AddLine(dataEntry, "Built a Kafka pipeline");
        var summaryEntry = cv.AddSection("Summary", SectionKind.Summary);
        cv.AddLine(summaryEntry, "Engineer.");
        var pinnedEntry = cv.AddSection("Languages", SectionKind.Custom, tags: [Tag.AlwaysOnName, "Data"]);
        cv.AddLine(pinnedEntry, "English, Japanese");

        var result = cv.Tailor("React developer");
        var decisions = result.Sections.ToDictionary(s => cv.Profile.Sections.Single(p => p.Id == s.SectionId).Title, s => s.Decision);

        Assert.Equal(SectionDecision.Unmatched, decisions["Data Projects"]);
        Assert.Equal(SectionDecision.Untagged, decisions["Summary"]);
        Assert.Equal(SectionDecision.AlwaysOn, decisions["Languages"]);
        Assert.Equal(LineDecision.SectionExcluded, Line(result, dataLine).Decision);
        Assert.False(Line(result, dataLine).Included);
    }

    [Fact]
    public void PinnedSections_KeepEveryLine()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var entry = cv.AddSection("Skills", SectionKind.Skills, maxLines: 1, tags: Tag.AlwaysOnName);
        var lines = new[] { cv.AddLine(entry, "Frontend: React"), cv.AddLine(entry, "Data: Kafka", "Data"), cv.AddLine(entry, "Languages") };

        var result = cv.Tailor("React developer");

        Assert.All(lines, id => Assert.Equal(LineDecision.AlwaysOn, Line(result, id).Decision));
        Assert.All(lines, id => Assert.True(Line(result, id).Included));
        Assert.Empty(result.TrimOrder);
    }

    [Fact]
    public void TrimOrder_DropsGenericLinesFirstAndNeverEmptiesAnEntry()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var entry = cv.AddSection("Experience", SectionKind.Experience);
        var strong = cv.AddLine(entry, "Strong", "Frontend", "Backend");
        var weak = cv.AddLine(entry, "Weak", "Frontend");
        var genericA = cv.AddLine(entry, "Generic A");
        var genericB = cv.AddLine(entry, "Generic B");
        var pinned = cv.AddLine(entry, "Pinned", Tag.AlwaysOnName);
        var single = cv.AddLine(cv.AddSection("Education", SectionKind.Education), "Only line");

        var result = cv.Tailor("React and C#");

        Assert.Equal([genericB, genericA, weak, strong], result.TrimOrder);
        Assert.DoesNotContain(pinned, result.TrimOrder);
        Assert.DoesNotContain(single, result.TrimOrder);
    }

    [Fact]
    public void MissingProfile_IsNotFound()
    {
        using var database = new TestDatabase();

        var result = database.Get<ITailoringFactory>().Tailor(new TailorRequest { ProfileId = 99, JobDescription = "x" });

        Assert.Equal(FactoryStatus.NotFound, result.Status);
    }
}
