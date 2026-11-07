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
    public async Task Restore_PutsTheProfileBackExactly()
    {
        using var database = new TestDatabase();
        var factory = database.Get<IProfileFactory>();
        var profile = await database.CreateProfile();
        await factory.Update(profile.Id, new ProfileRequest { Name = "Engineering", FullName = "Alex", Links = ["github.com/alex"] });
        profile = await database.AddTag(profile.Id, "Backend", "c#");
        var backend = profile.Tags.Single(t => t.Name == "Backend").Id;
        var alwaysOn = profile.Tags.Single(t => t.IsAlwaysOn).Id;
        profile = await database.AddSection(profile.Id);
        await database.Get<IEntryFactory>().Update(profile.Sections[0].Entries[0].Id, new EntryRequest { Heading = "Acme" });
        await database.AddLine(profile.Sections[0].Entries[0].Id, "Built **APIs**", backend, alwaysOn);
        profile = await database.AddLine(profile.Sections[0].Entries[0].Id, "Hidden line");
        await database.Get<ILineFactory>().Update(profile.Sections[0].Entries[0].Lines[1].Id, new LineRequest { Text = "Hidden line", Override = LineOverride.Hidden });
        await database.Get<ISectionFactory>().SetTags(profile.Sections[0].Id, new TagLinksRequest { TagIds = [backend] });
        await factory.SaveJobDescription(profile.Id, new JobDescriptionRequest { JobDescription = "C# role" });
        var snapshot = (await factory.GetById(profile.Id)).Value!;

        await database.Get<ISectionFactory>().Delete(snapshot.Sections[0].Id);
        await database.Get<ITagFactory>().Update(backend, new TagRequest { Name = "Server" });
        await factory.Update(profile.Id, new ProfileRequest { Name = "Changed", FullName = "Someone else" });
        await factory.SaveJobDescription(profile.Id, new JobDescriptionRequest { JobDescription = "" });

        var restored = (await factory.Restore(profile.Id, snapshot)).Value!;

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
    public async Task Duplicate_CopiesTheJobDescription()
    {
        using var database = new TestDatabase();
        var factory = database.Get<IProfileFactory>();
        var profile = await database.CreateProfile();
        await factory.SaveJobDescription(profile.Id, new JobDescriptionRequest { JobDescription = "  Frontend role  " });

        var copy = (await factory.Duplicate(profile.Id)).Value!;

        Assert.Equal("Frontend role", (await factory.GetById(profile.Id)).Value!.JobDescription);
        Assert.Equal("Frontend role", copy.JobDescription);
        Assert.Equal(FactoryStatus.NotFound, (await factory.SaveJobDescription(99, new JobDescriptionRequest())).Status);
    }

    [Fact]
    public async Task Schema_AddsTheJobDescriptionColumnToOlderDatabases()
    {
        using var database = new TestDatabase();
        using (var connection = await database.Connections.Create())
        {
            connection.Execute("ALTER TABLE Profiles DROP COLUMN JobDescription");
            Assert.Equal(0, connection.ExecuteScalar<int>("SELECT COUNT(*) FROM pragma_table_info('Profiles') WHERE name = 'JobDescription'"));
        }

        await database.Get<ISchemaRepository>().EnsureCreated();
        var profile = await database.CreateProfile();

        Assert.Equal(string.Empty, profile.JobDescription);
    }
}
