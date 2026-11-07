using Dapper;
using SmartCv.Factories;
using SmartCv.Models;
using SmartCv.Repositories;
using static SmartCv.Tests.TailoringFactoryTests;

namespace SmartCv.Tests;

public class PriorityTests
{
    private static async Task SetLinePriority(TestDatabase database, int lineId, int priority)
    {
        var line = (await database.Get<ILineRepository>().GetById(lineId))!;
        await database.Get<ILineFactory>().Update(lineId, new LineRequest { Text = line.Text, Override = line.Override, Priority = priority });
    }

    private static async Task SetSectionPriority(TestDatabase database, int sectionId, int priority)
    {
        var section = (await database.Get<ISectionRepository>().GetById(sectionId))!;
        await database.Get<ISectionFactory>().Update(sectionId, new SectionRequest { Title = section.Title, Kind = section.Kind, MaxLines = section.MaxLines, Priority = priority });
    }

    [Fact]
    public async Task Priority_IsSavedAndDefaultsToNormal()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience);
        var line = await cv.AddLine(entry, "A line");

        Assert.Equal(Priorities.Normal, (await cv.Profile()).Sections[0].Priority);
        Assert.Equal(Priorities.Normal, (await cv.Profile()).Sections[0].Entries[0].Lines[0].Priority);

        await SetLinePriority(database, line, Priorities.Highest);
        await SetSectionPriority(database, (await cv.Profile()).Sections[0].Id, Priorities.Lowest);

        Assert.Equal(Priorities.Highest, (await cv.Profile()).Sections[0].Entries[0].Lines[0].Priority);
        Assert.Equal(Priorities.Lowest, (await cv.Profile()).Sections[0].Priority);
        Assert.Equal(Priorities.Highest, (await database.Get<IProfileFactory>().Duplicate(cv.ProfileId)).Value!.Sections[0].Entries[0].Lines[0].Priority);
    }

    [Fact]
    public void Priority_RejectsOutOfRangeValues()
    {
        var request = new LineRequest { Text = "x", Priority = 6 };
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var valid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, new(request), results, true);

        Assert.False(valid);
    }

    [Fact]
    public async Task HigherPriority_WinsTheSlotsOverAHigherScore()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience, maxLines: 2);
        var strong = await cv.AddLine(entry, "Strong match", "Frontend", "Backend");
        var weakButImportant = await cv.AddLine(entry, "Weak match", "Frontend");
        var middle = await cv.AddLine(entry, "Middle match", "Backend");
        var irrelevant = await cv.AddLine(entry, "Data only", "Data");
        await SetLinePriority(database, weakButImportant, Priorities.Highest);
        await SetLinePriority(database, irrelevant, Priorities.Highest);

        var result = await cv.Tailor("React and C# and SQL");

        Assert.True(Line(result, weakButImportant).Included);
        Assert.True(Line(result, strong).Included);
        Assert.Equal(LineDecision.OverLimit, Line(result, middle).Decision);
        Assert.False(Line(result, irrelevant).Included);
        Assert.Equal(Priorities.Highest, Line(result, weakButImportant).Priority);
    }

    [Fact]
    public async Task TrimOrder_FollowsSectionThenLinePriority()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var workEntry = await cv.AddSection("Experience", SectionKind.Experience);
        var workA = await cv.AddLine(workEntry, "Work A", "Frontend");
        var workB = await cv.AddLine(workEntry, "Work B", "Frontend");
        var workC = await cv.AddLine(workEntry, "Work C", "Frontend");
        var skillsEntry = await cv.AddSection("Skills", SectionKind.Skills);
        var skillA = await cv.AddLine(skillsEntry, "Skill A", "Frontend", "Backend");
        var skillB = await cv.AddLine(skillsEntry, "Skill B", "Frontend", "Backend");
        await SetSectionPriority(database, (await cv.Profile()).Sections.Single(s => s.Title == "Skills").Id, Priorities.Lowest);
        await SetLinePriority(database, workA, 4);
        await SetLinePriority(database, workC, 2);

        var result = await cv.Tailor("React and C#");

        Assert.Equal([skillB, workC, workB], result.TrimOrder);
        Assert.DoesNotContain(workA, result.TrimOrder);
        Assert.DoesNotContain(skillA, result.TrimOrder);
    }

    [Fact]
    public async Task Schema_AddsPriorityColumnsToOlderDatabases()
    {
        using var database = new TestDatabase();
        using (var connection = await database.Connections.Create())
        {
            connection.Execute("ALTER TABLE Lines DROP COLUMN Priority");
            connection.Execute("ALTER TABLE Sections DROP COLUMN Priority");
        }

        await database.Get<ISchemaRepository>().EnsureCreated();
        var cv = await CreateCv(database);
        await cv.AddLine(await cv.AddSection("Experience", SectionKind.Experience), "Line");

        Assert.Equal(Priorities.Normal, (await cv.Profile()).Sections[0].Priority);
        Assert.Equal(Priorities.Normal, (await cv.Profile()).Sections[0].Entries[0].Lines[0].Priority);
    }
}
