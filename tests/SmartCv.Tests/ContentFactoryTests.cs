using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public class ContentFactoryTests
{
    [Fact]
    public async Task CreateSection_AddsAnEmptyEntryAndAppendsInOrder()
    {
        using var database = new TestDatabase();
        var profile = await database.CreateProfile();

        await database.AddSection(profile.Id, "Summary", SectionKind.Summary);
        profile = await database.AddSection(profile.Id, "Skills", SectionKind.Skills);

        Assert.Equal(["Summary", "Skills"], profile.Sections.Select(s => s.Title));
        Assert.All(profile.Sections, section => Assert.Single(section.Entries));
    }

    [Fact]
    public async Task ReorderSections_RequiresEverySectionOnce()
    {
        using var database = new TestDatabase();
        var profile = await database.CreateProfile();
        await database.AddSection(profile.Id, "A");
        profile = await database.AddSection(profile.Id, "B");
        var ids = profile.Sections.Select(s => s.Id).ToList();
        var factory = database.Get<ISectionFactory>();

        var partial = await factory.Reorder(new ReorderRequest { ParentId = profile.Id, Ids = [ids[0]] });
        var duplicated = await factory.Reorder(new ReorderRequest { ParentId = profile.Id, Ids = [ids[0], ids[0]] });
        var reordered = await factory.Reorder(new ReorderRequest { ParentId = profile.Id, Ids = [ids[1], ids[0]] });

        Assert.Equal(FactoryStatus.Invalid, partial.Status);
        Assert.Equal(FactoryStatus.Invalid, duplicated.Status);
        Assert.Equal(["B", "A"], reordered.Value!.Sections.Select(s => s.Title));
    }

    [Fact]
    public async Task Entries_CanBeAddedUpdatedReorderedAndDeleted()
    {
        using var database = new TestDatabase();
        var profile = await database.CreateProfile();
        profile = await database.AddSection(profile.Id);
        var section = profile.Sections[0];
        var factory = database.Get<IEntryFactory>();

        profile = (await factory.Create(new CreateEntryRequest { SectionId = section.Id, Heading = "StayinFront" })).Value!;
        var entries = profile.Sections[0].Entries;
        profile = (await factory.Update(entries[0].Id, new EntryRequest { Heading = "Black Pearl Group", Subheading = "Developer", DateRange = "Sep 2024 – Present" })).Value!;
        profile = (await factory.Reorder(new ReorderRequest { ParentId = section.Id, Ids = [entries[1].Id, entries[0].Id] })).Value!;

        Assert.Equal(["StayinFront", "Black Pearl Group"], profile.Sections[0].Entries.Select(e => e.Heading));
        Assert.Equal("Sep 2024 – Present", profile.Sections[0].Entries[1].DateRange);

        profile = (await factory.Delete(entries[1].Id)).Value!;
        Assert.Equal(["Black Pearl Group"], profile.Sections[0].Entries.Select(e => e.Heading));
    }

    [Fact]
    public async Task Lines_SaveOverrideAndReorder()
    {
        using var database = new TestDatabase();
        var profile = await database.CreateProfile();
        profile = await database.AddSection(profile.Id);
        var entryId = profile.Sections[0].Entries[0].Id;
        await database.AddLine(entryId, "First");
        profile = await database.AddLine(entryId, "Second");
        var lines = profile.Sections[0].Entries[0].Lines;
        var factory = database.Get<ILineFactory>();

        await factory.Update(lines[0].Id, new LineRequest { Text = "First, edited", Override = LineOverride.Pinned });
        profile = (await factory.Reorder(new ReorderRequest { ParentId = entryId, Ids = [lines[1].Id, lines[0].Id] })).Value!;

        var saved = profile.Sections[0].Entries[0].Lines;
        Assert.Equal(["Second", "First, edited"], saved.Select(l => l.Text));
        Assert.Equal(LineOverride.Pinned, saved[1].Override);
    }

    [Fact]
    public async Task LineTags_MustBelongToTheSameProfile()
    {
        using var database = new TestDatabase();
        var other = await database.CreateProfile("Other");
        var foreignTag = (await database.AddTag(other.Id, "Foreign")).Tags.Single(t => t.Name == "Foreign");
        var profile = await database.CreateProfile();
        profile = await database.AddSection(profile.Id);
        var entryId = profile.Sections[0].Entries[0].Id;
        profile = await database.AddLine(entryId, "A line");
        var lineId = profile.Sections[0].Entries[0].Lines[0].Id;
        var factory = database.Get<ILineFactory>();

        var setResult = await factory.SetTags(lineId, new TagLinksRequest { TagIds = [foreignTag.Id] });
        var createResult = await factory.Create(new CreateLineRequest { EntryId = entryId, Text = "B", TagIds = [foreignTag.Id] });
        var sectionResult = await database.Get<ISectionFactory>().SetTags(profile.Sections[0].Id, new TagLinksRequest { TagIds = [foreignTag.Id] });

        Assert.Equal(FactoryStatus.Invalid, setResult.Status);
        Assert.Equal(FactoryStatus.Invalid, createResult.Status);
        Assert.Equal(FactoryStatus.Invalid, sectionResult.Status);
    }

    [Fact]
    public async Task MissingParents_ReturnNotFound()
    {
        using var database = new TestDatabase();

        Assert.Equal(FactoryStatus.NotFound, (await database.Get<ISectionFactory>().Create(new CreateSectionRequest { ProfileId = 99, Title = "X" })).Status);
        Assert.Equal(FactoryStatus.NotFound, (await database.Get<IEntryFactory>().Create(new CreateEntryRequest { SectionId = 99 })).Status);
        Assert.Equal(FactoryStatus.NotFound, (await database.Get<ILineFactory>().Create(new CreateLineRequest { EntryId = 99 })).Status);
        Assert.Equal(FactoryStatus.NotFound, (await database.Get<ILineFactory>().Update(99, new LineRequest())).Status);
        Assert.Equal(FactoryStatus.NotFound, (await database.Get<ITagFactory>().GetByProfileId(99)).Status);
    }
}
