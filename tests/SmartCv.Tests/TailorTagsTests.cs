using SmartCv.Factories;
using SmartCv.Models;
using static SmartCv.Tests.TailoringFactoryTests;

namespace SmartCv.Tests;

public class TailorTagsTests
{
    private static async Task<TailorResult> TailorByTags(Cv cv, params string[] tags) =>
        (await cv.Database.Get<ITailoringFactory>()
            .Tailor(new TailorRequest { ProfileId = cv.ProfileId, TagIds = tags.Select(cv.TagId).ToList() }, CancellationToken.None))
            .Value!;

    [Fact]
    public async Task ChosenTags_AreWeightedByTheirOrder()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience, maxLines: 3);
        var dataOnly = await cv.AddLine(entry, "Data only", "Data");
        var frontendOnly = await cv.AddLine(entry, "Frontend only", "Frontend");
        var both = await cv.AddLine(entry, "Both", "Frontend", "Data");
        var backendOnly = await cv.AddLine(entry, "Backend only", "Backend");
        var untagged = await cv.AddLine(entry, "Untagged");

        var result = await TailorByTags(cv, "Frontend", "Data");

        Assert.Equal(TailorSource.Tags, result.Source);
        Assert.Equal([("Frontend", 2), ("Data", 1)], result.MatchedTags.Select(tag => (tag.Name, tag.Score)));
        Assert.Equal((3, true), (Line(result, both).Score, Line(result, both).Included));
        Assert.Equal((2, true), (Line(result, frontendOnly).Score, Line(result, frontendOnly).Included));
        Assert.Equal((1, true), (Line(result, dataOnly).Score, Line(result, dataOnly).Included));
        Assert.Equal(LineDecision.Unmatched, Line(result, backendOnly).Decision);
        Assert.Equal(LineDecision.OverLimit, Line(result, untagged).Decision);
        Assert.Equal([dataOnly, frontendOnly], result.TrimOrder);
    }

    [Fact]
    public async Task ReversingTheOrder_ChangesWhoWins()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var entry = await cv.AddSection("Experience", SectionKind.Experience, maxLines: 1);
        var frontend = await cv.AddLine(entry, "Frontend only", "Frontend");
        var data = await cv.AddLine(entry, "Data only", "Data");

        Assert.True(Line(await TailorByTags(cv, "Frontend", "Data"), frontend).Included);
        Assert.True(Line(await TailorByTags(cv, "Data", "Frontend"), data).Included);
    }

    [Fact]
    public async Task Tags_WinOverAJobDescription_AndOneOfThemIsRequired()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var factory = database.Get<ITailoringFactory>();

        var both = (await factory.Tailor(new TailorRequest { ProfileId = cv.ProfileId, JobDescription = "React", TagIds = [cv.TagId("Data")] }, CancellationToken.None)).Value!;
        var neither = (await factory.Tailor(new TailorRequest { ProfileId = cv.ProfileId }, CancellationToken.None));
        var onlyAlwaysOn = (await factory.Tailor(new TailorRequest { ProfileId = cv.ProfileId, TagIds = [cv.AlwaysOn] }, CancellationToken.None));

        Assert.Equal(TailorSource.Tags, both.Source);
        Assert.Equal(["Data"], both.MatchedTags.Select(tag => tag.Name));
        Assert.Equal(FactoryStatus.Invalid, neither.Status);
        Assert.Equal(FactoryStatus.Invalid, onlyAlwaysOn.Status);
    }

    [Fact]
    public async Task TailorTags_AreSavedInOrderAndFollowTagChanges()
    {
        using var database = new TestDatabase();
        var cv = await CreateCv(database);
        var profiles = database.Get<IProfileFactory>();

        var saved = await profiles.SaveTailorTags(cv.ProfileId, new TagLinksRequest { TagIds = [cv.TagId("Data"), cv.TagId("Frontend")] });
        var withAlwaysOn = await profiles.SaveTailorTags(cv.ProfileId, new TagLinksRequest { TagIds = [cv.AlwaysOn] });

        Assert.Equal(FactoryStatus.Success, saved.Status);
        Assert.Equal(FactoryStatus.Invalid, withAlwaysOn.Status);
        Assert.Equal([cv.TagId("Data"), cv.TagId("Frontend")], (await cv.Profile()).TailorTagIds);

        var copy = (await profiles.Duplicate(cv.ProfileId)).Value!;
        Assert.Equal(["Data", "Frontend"], copy.TailorTagIds.Select(id => copy.Tags.Single(t => t.Id == id).Name));

        await database.Get<ITagFactory>().Delete(cv.TagId("Data"));
        Assert.Equal([cv.TagId("Frontend")], (await cv.Profile()).TailorTagIds);

        var snapshot = await cv.Profile();
        await profiles.SaveTailorTags(cv.ProfileId, new TagLinksRequest { TagIds = [] });
        var restored = (await profiles.Restore(cv.ProfileId, snapshot)).Value!;
        Assert.Equal(["Frontend"], restored.TailorTagIds.Select(id => restored.Tags.Single(t => t.Id == id).Name));
    }
}
