using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public class ProfileFactoryTests
{
    [Fact]
    public void Create_UsesDefaultTemplateAndAddsAlwaysOnTag()
    {
        using var database = new TestDatabase();

        var profile = database.CreateProfile();

        Assert.Equal(TemplateSummary.DefaultName, profile.TemplateName);
        Assert.Equal(1, profile.TargetPages);
        var tag = Assert.Single(profile.Tags);
        Assert.True(tag.IsAlwaysOn);
        Assert.Equal(Tag.AlwaysOnName, tag.Name);
        Assert.InRange(profile.CreatedAt.ToUniversalTime(), DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void Create_RejectsMissingTemplate()
    {
        using var database = new TestDatabase();

        var result = database.Get<IProfileFactory>().Create(new ProfileRequest { Name = "Engineering", TemplateName = "missing" });

        Assert.Equal(FactoryStatus.Invalid, result.Status);
    }

    [Fact]
    public void Update_SavesContactDetailsAndLinks()
    {
        using var database = new TestDatabase();
        var profile = database.CreateProfile();

        var result = database.Get<IProfileFactory>().Update(profile.Id, new ProfileRequest
        {
            Name = " Backend ",
            TemplateName = "modern",
            TargetPages = 2,
            FullName = "Alex Smith",
            Email = "alex@example.com",
            Links = ["github.com/alex", " ", "linkedin.com/in/alex"],
        });

        Assert.Equal(FactoryStatus.Success, result.Status);
        var updated = database.Get<IProfileFactory>().GetById(profile.Id).Value!;
        Assert.Equal("Backend", updated.Name);
        Assert.Equal("modern", updated.TemplateName);
        Assert.Equal(2, updated.TargetPages);
        Assert.Equal("Alex Smith", updated.FullName);
        Assert.Equal(["github.com/alex", "linkedin.com/in/alex"], updated.Links);
    }

    [Fact]
    public void GetById_AssemblesTheFullTree()
    {
        using var database = new TestDatabase();
        var profile = database.CreateProfile();
        profile = database.AddTag(profile.Id, "Backend", "C#", "sql");
        var tag = profile.Tags.Single(t => t.Name == "Backend");
        profile = database.AddSection(profile.Id);
        var entry = profile.Sections.Single().Entries.Single();
        database.AddLine(entry.Id, "Built the **sync service**", tag.Id);
        profile = database.AddLine(entry.Id, "Cut load time by 50%");

        var section = Assert.Single(profile.Sections);
        Assert.Equal(SectionKind.Experience, section.Kind);
        var lines = Assert.Single(section.Entries).Lines;
        Assert.Equal(["Built the **sync service**", "Cut load time by 50%"], lines.Select(l => l.Text));
        Assert.Equal([tag.Id], lines[0].TagIds);
        Assert.Empty(lines[1].TagIds);
        Assert.Equal(["c#", "sql"], profile.Tags.Single(t => t.Id == tag.Id).Keywords);
        Assert.Equal(1, profile.Tags.Single(t => t.Id == tag.Id).UsageCount);
    }

    [Fact]
    public void Duplicate_CopiesEverythingWithNewIds()
    {
        using var database = new TestDatabase();
        var profile = database.CreateProfile();
        profile = database.AddTag(profile.Id, "Frontend", "react");
        var tag = profile.Tags.Single(t => t.Name == "Frontend");
        profile = database.AddSection(profile.Id);
        database.Get<ISectionFactory>().SetTags(profile.Sections[0].Id, new TagLinksRequest { TagIds = [tag.Id] });
        database.AddLine(profile.Sections[0].Entries[0].Id, "Built a design system", tag.Id);

        var result = database.Get<IProfileFactory>().Duplicate(profile.Id);

        Assert.Equal(FactoryStatus.Success, result.Status);
        var copy = result.Value!;
        Assert.NotEqual(profile.Id, copy.Id);
        Assert.Equal("Engineering (copy)", copy.Name);
        var copiedTag = copy.Tags.Single(t => t.Name == "Frontend");
        Assert.NotEqual(tag.Id, copiedTag.Id);
        Assert.Contains(copy.Tags, t => t.IsAlwaysOn);
        Assert.Equal([copiedTag.Id], copy.Sections[0].TagIds);
        var line = copy.Sections[0].Entries[0].Lines.Single();
        Assert.Equal("Built a design system", line.Text);
        Assert.Equal([copiedTag.Id], line.TagIds);
    }

    [Fact]
    public void Delete_RemovesProfileAndChildren()
    {
        using var database = new TestDatabase();
        var profile = database.CreateProfile();
        profile = database.AddSection(profile.Id);
        database.AddLine(profile.Sections[0].Entries[0].Id, "A line");

        var result = database.Get<IProfileFactory>().Delete(profile.Id);

        Assert.Equal(FactoryStatus.Success, result.Status);
        Assert.Equal(FactoryStatus.NotFound, database.Get<IProfileFactory>().GetById(profile.Id).Status);
        Assert.Equal(FactoryStatus.NotFound, database.Get<IProfileFactory>().Delete(profile.Id).Status);
    }
}
