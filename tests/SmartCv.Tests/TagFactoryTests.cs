using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public class TagFactoryTests
{
    [Fact]
    public async Task AlwaysOnTag_CannotBeRenamedOrDeleted()
    {
        using var database = new TestDatabase();
        var profile = await database.CreateProfile();
        var alwaysOn = profile.Tags.Single(t => t.IsAlwaysOn);
        var factory = database.Get<ITagFactory>();

        var rename = await factory.Update(alwaysOn.Id, new TagRequest { Name = "Pinned" });
        var delete = await factory.Delete(alwaysOn.Id);
        var keepName = await factory.Update(alwaysOn.Id, new TagRequest { Name = Tag.AlwaysOnName, Colour = "#000000" });

        Assert.Equal(FactoryStatus.Invalid, rename.Status);
        Assert.Equal(FactoryStatus.Invalid, delete.Status);
        Assert.Equal(FactoryStatus.Success, keepName.Status);
    }

    [Fact]
    public async Task Names_AreUniquePerProfileIgnoringCase()
    {
        using var database = new TestDatabase();
        var profile = await database.CreateProfile();
        var other = await database.CreateProfile("Other");
        await database.AddTag(profile.Id, "Backend");
        var factory = database.Get<ITagFactory>();

        var duplicate = await factory.Create(new CreateTagRequest { ProfileId = profile.Id, Name = "backend" });
        var alwaysOnClash = await factory.Create(new CreateTagRequest { ProfileId = profile.Id, Name = "always on" });
        var otherProfile = await factory.Create(new CreateTagRequest { ProfileId = other.Id, Name = "Backend" });

        Assert.Equal(FactoryStatus.Invalid, duplicate.Status);
        Assert.Equal(FactoryStatus.Invalid, alwaysOnClash.Status);
        Assert.Equal(FactoryStatus.Success, otherProfile.Status);
    }

    [Fact]
    public async Task Update_NormalisesKeywords()
    {
        using var database = new TestDatabase();
        var profile = await database.AddTag((await database.CreateProfile()).Id, "Cloud");
        var tag = profile.Tags.Single(t => t.Name == "Cloud");

        var updated = (await database.Get<ITagFactory>().Update(tag.Id, new TagRequest { Name = "Cloud & Data", Keywords = [" Azure ", "azure", "", "Kafka"] })).Value!;

        var saved = updated.Tags.Single(t => t.Id == tag.Id);
        Assert.Equal("Cloud & Data", saved.Name);
        Assert.Equal(["azure", "kafka"], saved.Keywords);
    }

    [Fact]
    public async Task Delete_RemovesTagFromLines()
    {
        using var database = new TestDatabase();
        var profile = await database.CreateProfile();
        profile = await database.AddTag(profile.Id, "Go");
        var tag = profile.Tags.Single(t => t.Name == "Go");
        profile = await database.AddSection(profile.Id);
        await database.AddLine(profile.Sections[0].Entries[0].Id, "Certified in Go", tag.Id);

        profile = (await database.Get<ITagFactory>().Delete(tag.Id)).Value!;

        Assert.DoesNotContain(profile.Tags, t => t.Id == tag.Id);
        Assert.Empty(profile.Sections[0].Entries[0].Lines[0].TagIds);
    }
}
