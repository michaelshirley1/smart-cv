using Dapper;
using SmartCv.Factories;
using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Tests;

public class RestoreAndJobDescriptionTests
{
    private static string Describe(ProfileDetail profile) => string.Join(" | ",
        profile.Sections.Select(section =>
            $"{section.Title}[{string.Join(",", section.TagIds.Select(id => profile.Tags.Single(t => t.Id == id).Name))}]:" +
            string.Join(";", section.Entries.Select(entry =>
                $"{entry.Heading}=" + string.Join("/", entry.Lines.Select(line =>
                    $"{line.Text}({line.Override},{string.Join(",", line.TagIds.Select(id => profile.Tags.Single(t => t.Id == id).Name).Order())})"))))));

    [Fact]
    public void Restore_PutsTheProfileBackExactly()
    {
        using var database = new TestDatabase();
        var factory = database.Get<IProfileFactory>();
        var profile = database.CreateProfile();
        factory.Update(profile.Id, new ProfileRequest { Name = "Engineering", FullName = "Alex", Links = ["github.com/alex"] });
        profile = database.AddTag(profile.Id, "Backend", "c#");
        var backend = profile.Tags.Single(t => t.Name == "Backend").Id;
        var alwaysOn = profile.Tags.Single(t => t.IsAlwaysOn).Id;
        profile = database.AddSection(profile.Id);
        database.Get<IEntryFactory>().Update(profile.Sections[0].Entries[0].Id, new EntryRequest { Heading = "Acme" });
        database.AddLine(profile.Sections[0].Entries[0].Id, "Built **APIs**", backend, alwaysOn);
        profile = database.AddLine(profile.Sections[0].Entries[0].Id, "Hidden line");
        database.Get<ILineFactory>().Update(profile.Sections[0].Entries[0].Lines[1].Id, new LineRequest { Text = "Hidden line", Override = LineOverride.Hidden });
        database.Get<ISectionFactory>().SetTags(profile.Sections[0].Id, new TagLinksRequest { TagIds = [backend] });
        factory.SaveJobDescription(profile.Id, new JobDescriptionRequest { JobDescription = "C# role" });
        var snapshot = factory.GetById(profile.Id).Value!;

        database.Get<ISectionFactory>().Delete(snapshot.Sections[0].Id);
        database.Get<ITagFactory>().Update(backend, new TagRequest { Name = "Server" });
        factory.Update(profile.Id, new ProfileRequest { Name = "Changed", FullName = "Someone else" });
        factory.SaveJobDescription(profile.Id, new JobDescriptionRequest { JobDescription = "" });

        var restored = factory.Restore(profile.Id, snapshot).Value!;

        Assert.Equal(Describe(snapshot), Describe(restored));
        Assert.Equal("Engineering", restored.Name);
        Assert.Equal("Alex", restored.FullName);
        Assert.Equal(["github.com/alex"], restored.Links);
        Assert.Equal("C# role", restored.JobDescription);
        Assert.Equal(alwaysOn, restored.Tags.Single(t => t.IsAlwaysOn).Id);
        Assert.Equal(["c#"], restored.Tags.Single(t => t.Name == "Backend").Keywords);
        Assert.Equal(2, restored.Tags.Count);
    }

    [Fact]
    public void Duplicate_CopiesTheJobDescription()
    {
        using var database = new TestDatabase();
        var factory = database.Get<IProfileFactory>();
        var profile = database.CreateProfile();
        factory.SaveJobDescription(profile.Id, new JobDescriptionRequest { JobDescription = "  Frontend role  " });

        var copy = factory.Duplicate(profile.Id).Value!;

        Assert.Equal("Frontend role", factory.GetById(profile.Id).Value!.JobDescription);
        Assert.Equal("Frontend role", copy.JobDescription);
        Assert.Equal(FactoryStatus.NotFound, factory.SaveJobDescription(99, new JobDescriptionRequest()).Status);
    }

    [Fact]
    public void Schema_AddsTheJobDescriptionColumnToOlderDatabases()
    {
        using var database = new TestDatabase();
        using (var connection = database.Connections.Create())
        {
            connection.Execute("ALTER TABLE Profiles DROP COLUMN JobDescription");
            Assert.Equal(0, connection.ExecuteScalar<int>("SELECT COUNT(*) FROM pragma_table_info('Profiles') WHERE name = 'JobDescription'"));
        }

        database.Get<ISchemaRepository>().EnsureCreated();
        var profile = database.CreateProfile();

        Assert.Equal(string.Empty, profile.JobDescription);
    }
}
