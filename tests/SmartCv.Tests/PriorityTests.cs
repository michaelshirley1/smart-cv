using Dapper;
using SmartCv.Factories;
using SmartCv.Models;
using SmartCv.Repositories;
using static SmartCv.Tests.TailoringFactoryTests;

namespace SmartCv.Tests;

public class PriorityTests
{
    private static void SetLinePriority(TestDatabase database, int lineId, int priority)
    {
        var line = database.Get<ILineRepository>().GetById(lineId)!;
        database.Get<ILineFactory>().Update(lineId, new LineRequest { Text = line.Text, Override = line.Override, Priority = priority });
    }

    private static void SetSectionPriority(TestDatabase database, int sectionId, int priority)
    {
        var section = database.Get<ISectionRepository>().GetById(sectionId)!;
        database.Get<ISectionFactory>().Update(sectionId, new SectionRequest { Title = section.Title, Kind = section.Kind, MaxLines = section.MaxLines, Priority = priority });
    }

    [Fact]
    public void Priority_IsSavedAndDefaultsToNormal()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var entry = cv.AddSection("Experience", SectionKind.Experience);
        var line = cv.AddLine(entry, "A line");

        Assert.Equal(Priorities.Normal, cv.Profile.Sections[0].Priority);
        Assert.Equal(Priorities.Normal, cv.Profile.Sections[0].Entries[0].Lines[0].Priority);

        SetLinePriority(database, line, Priorities.Highest);
        SetSectionPriority(database, cv.Profile.Sections[0].Id, Priorities.Lowest);

        Assert.Equal(Priorities.Highest, cv.Profile.Sections[0].Entries[0].Lines[0].Priority);
        Assert.Equal(Priorities.Lowest, cv.Profile.Sections[0].Priority);
        Assert.Equal(Priorities.Highest, database.Get<IProfileFactory>().Duplicate(cv.ProfileId).Value!.Sections[0].Entries[0].Lines[0].Priority);
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
    public void HigherPriority_WinsTheSlotsOverAHigherScore()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var entry = cv.AddSection("Experience", SectionKind.Experience, maxLines: 2);
        var strong = cv.AddLine(entry, "Strong match", "Frontend", "Backend");
        var weakButImportant = cv.AddLine(entry, "Weak match", "Frontend");
        var middle = cv.AddLine(entry, "Middle match", "Backend");
        var irrelevant = cv.AddLine(entry, "Data only", "Data");
        SetLinePriority(database, weakButImportant, Priorities.Highest);
        SetLinePriority(database, irrelevant, Priorities.Highest);

        var result = cv.Tailor("React and C# and SQL");

        Assert.True(Line(result, weakButImportant).Included);
        Assert.True(Line(result, strong).Included);
        Assert.Equal(LineDecision.OverLimit, Line(result, middle).Decision);
        Assert.False(Line(result, irrelevant).Included);
        Assert.Equal(Priorities.Highest, Line(result, weakButImportant).Priority);
    }

    [Fact]
    public void TrimOrder_FollowsSectionThenLinePriority()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var workEntry = cv.AddSection("Experience", SectionKind.Experience);
        var workA = cv.AddLine(workEntry, "Work A", "Frontend");
        var workB = cv.AddLine(workEntry, "Work B", "Frontend");
        var workC = cv.AddLine(workEntry, "Work C", "Frontend");
        var skillsEntry = cv.AddSection("Skills", SectionKind.Skills);
        var skillA = cv.AddLine(skillsEntry, "Skill A", "Frontend", "Backend");
        var skillB = cv.AddLine(skillsEntry, "Skill B", "Frontend", "Backend");
        SetSectionPriority(database, cv.Profile.Sections.Single(s => s.Title == "Skills").Id, Priorities.Lowest);
        SetLinePriority(database, workA, 4);
        SetLinePriority(database, workC, 2);

        var result = cv.Tailor("React and C#");

        Assert.Equal([skillB, workC, workB], result.TrimOrder);
        Assert.DoesNotContain(workA, result.TrimOrder);
        Assert.DoesNotContain(skillA, result.TrimOrder);
    }

    [Fact]
    public void Schema_AddsPriorityColumnsToOlderDatabases()
    {
        using var database = new TestDatabase();
        using (var connection = database.Connections.Create())
        {
            connection.Execute("ALTER TABLE Lines DROP COLUMN Priority");
            connection.Execute("ALTER TABLE Sections DROP COLUMN Priority");
        }

        database.Get<ISchemaRepository>().EnsureCreated();
        var cv = CreateCv(database);
        cv.AddLine(cv.AddSection("Experience", SectionKind.Experience), "Line");

        Assert.Equal(Priorities.Normal, cv.Profile.Sections[0].Priority);
        Assert.Equal(Priorities.Normal, cv.Profile.Sections[0].Entries[0].Lines[0].Priority);
    }
}
