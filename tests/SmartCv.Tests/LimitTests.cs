using Dapper;
using SmartCv.Factories;
using SmartCv.Models;
using SmartCv.Repositories;
using static SmartCv.Tests.TailoringFactoryTests;

namespace SmartCv.Tests;

public class LimitTests
{
    private static async Task SetLimits(TestDatabase database, int sectionId, int maxLines, int maxTotal)
    {
        var section = (await database.Get<ISectionRepository>().GetById(sectionId))!;
        var result = await database.Get<ISectionFactory>().Update(sectionId, new SectionRequest { Title = section.Title, Kind = section.Kind, MaxLines = maxLines, MaxTotal = maxTotal, Priority = section.Priority });
        Assert.Equal(FactoryStatus.Success, result.Status);
    }

    private static async Task SetPriority(TestDatabase database, int lineId, int priority)
    {
        var line = (await database.Get<ILineRepository>().GetById(lineId))!;
        await database.Get<ILineFactory>().Update(lineId, new LineRequest { Text = line.Text, Override = line.Override, Priority = priority });
    }

    [Fact]
    public async Task MaxPerEntryOfZero_MeansNoLimit()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience);
        var lines = new List<int>();
        foreach (var i in Enumerable.Range(1, 9)) lines.Add(await cv.AddLine(entry, $"Line {i}", "Frontend"));
        await SetLimits(database, (await cv.Profile()).Sections[0].Id, 0, 0);

        var result = await cv.Tailor("React");

        Assert.All(lines, id => Assert.True(Line(result, id).Included));
    }

    [Fact]
    public async Task MaxInSection_CapsTheWholeSectionAndKeepsOnePerEntry()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var first = await cv.AddSection("Experience", SectionKind.Experience);
        var sectionId = (await cv.Profile()).Sections[0].Id;
        var second = (await database.Get<IEntryFactory>().Create(new CreateEntryRequest { SectionId = sectionId })).Value!.Sections[0].Entries[1].Id;

        var strong = await cv.AddLine(first, "Strong", "Frontend", "Backend");
        var weak = await cv.AddLine(first, "Weak", "Frontend");
        var important = await cv.AddLine(first, "Important", "Frontend");
        var pinned = await cv.AddLine(first, "Pinned", Tag.AlwaysOnName);
        var onlyLine = await cv.AddLine(second, "Only line in the second job", "Frontend");
        await SetPriority(database, important, Priorities.Highest);
        await SetLimits(database, sectionId, 5, 3);

        var result = await cv.Tailor("React and C#");

        Assert.True(Line(result, pinned).Included);
        Assert.True(Line(result, onlyLine).Included);
        Assert.True(Line(result, important).Included);
        Assert.Equal(LineDecision.OverLimit, Line(result, weak).Decision);
        Assert.Equal(LineDecision.OverLimit, Line(result, strong).Decision);
        Assert.Equal(3, result.Lines.Count(line => line.SectionId == sectionId && line.Included));
    }

    [Fact]
    public async Task Limits_AreSavedAndValidated()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        await cv.AddSection("Experience", SectionKind.Experience);
        await SetLimits(database, (await cv.Profile()).Sections[0].Id, 0, 12);

        Assert.Equal((0, 12), ((await cv.Profile()).Sections[0].MaxLines, (await cv.Profile()).Sections[0].MaxTotal));
        Assert.Equal(12, (await database.Get<IProfileFactory>().Duplicate(cv.ProfileId)).Value!.Sections[0].MaxTotal);

        var tooMany = new SectionRequest { Title = "X", MaxTotal = 500 };
        var noPageLimit = new ProfileRequest { Name = "X", TargetPages = 0 };
        Assert.False(System.ComponentModel.DataAnnotations.Validator.TryValidateObject(tooMany, new(tooMany), [], true));
        Assert.True(System.ComponentModel.DataAnnotations.Validator.TryValidateObject(noPageLimit, new(noPageLimit), [], true));
    }

    [Fact]
    public async Task Schema_AddsMaxTotalToOlderDatabases()
    {
        using var database = new TestDatabase();
        using (var connection = await database.Connections.Create())
        {
            connection.Execute("ALTER TABLE Sections DROP COLUMN MaxTotal");
        }

        await database.Get<ISchemaRepository>().EnsureCreated();
        var cv = await CreateCv(database);
        await cv.AddSection("Experience", SectionKind.Experience);

        Assert.Equal(0, (await cv.Profile()).Sections[0].MaxTotal);
    }
}
