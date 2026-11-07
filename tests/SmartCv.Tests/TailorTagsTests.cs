using SmartCv.Factories;
using SmartCv.Models;
using static SmartCv.Tests.TailoringFactoryTests;

namespace SmartCv.Tests;

public class TailorTagsTests
{
    private static TailorResult TailorByTags(Cv cv, params string[] tags) =>
        cv.Database.Get<ITailoringFactory>()
            .TailorAsync(new TailorRequest { ProfileId = cv.ProfileId, TagIds = tags.Select(cv.TagId).ToList() }, CancellationToken.None)
            .Result.Value!;

    [Fact]
    public void ChosenTags_AreWeightedByTheirOrder()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var entry = cv.AddSection("Experience", SectionKind.Experience, maxLines: 3);
        var dataOnly = cv.AddLine(entry, "Data only", "Data");
        var frontendOnly = cv.AddLine(entry, "Frontend only", "Frontend");
        var both = cv.AddLine(entry, "Both", "Frontend", "Data");
        var backendOnly = cv.AddLine(entry, "Backend only", "Backend");
        var untagged = cv.AddLine(entry, "Untagged");

        var result = TailorByTags(cv, "Frontend", "Data");

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
    public void ReversingTheOrder_ChangesWhoWins()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var entry = cv.AddSection("Experience", SectionKind.Experience, maxLines: 1);
        var frontend = cv.AddLine(entry, "Frontend only", "Frontend");
        var data = cv.AddLine(entry, "Data only", "Data");

        Assert.True(Line(TailorByTags(cv, "Frontend", "Data"), frontend).Included);
        Assert.True(Line(TailorByTags(cv, "Data", "Frontend"), data).Included);
    }

    [Fact]
    public void Tags_WinOverAJobDescription_AndOneOfThemIsRequired()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var factory = database.Get<ITailoringFactory>();

        var both = factory.TailorAsync(new TailorRequest { ProfileId = cv.ProfileId, JobDescription = "React", TagIds = [cv.TagId("Data")] }, CancellationToken.None).Result.Value!;
        var neither = factory.TailorAsync(new TailorRequest { ProfileId = cv.ProfileId }, CancellationToken.None).Result;
        var onlyAlwaysOn = factory.TailorAsync(new TailorRequest { ProfileId = cv.ProfileId, TagIds = [cv.AlwaysOn] }, CancellationToken.None).Result;

        Assert.Equal(TailorSource.Tags, both.Source);
        Assert.Equal(["Data"], both.MatchedTags.Select(tag => tag.Name));
        Assert.Equal(FactoryStatus.Invalid, neither.Status);
        Assert.Equal(FactoryStatus.Invalid, onlyAlwaysOn.Status);
    }

    [Fact]
    public void TailorTags_AreSavedInOrderAndFollowTagChanges()
    {
        using var database = new TestDatabase();
        var cv = CreateCv(database);
        var profiles = database.Get<IProfileFactory>();

        var saved = profiles.SaveTailorTags(cv.ProfileId, new TagLinksRequest { TagIds = [cv.TagId("Data"), cv.TagId("Frontend")] });
        var withAlwaysOn = profiles.SaveTailorTags(cv.ProfileId, new TagLinksRequest { TagIds = [cv.AlwaysOn] });

        Assert.Equal(FactoryStatus.Success, saved.Status);
        Assert.Equal(FactoryStatus.Invalid, withAlwaysOn.Status);
        Assert.Equal([cv.TagId("Data"), cv.TagId("Frontend")], cv.Profile.TailorTagIds);

        var copy = profiles.Duplicate(cv.ProfileId).Value!;
        Assert.Equal(["Data", "Frontend"], copy.TailorTagIds.Select(id => copy.Tags.Single(t => t.Id == id).Name));

        database.Get<ITagFactory>().Delete(cv.TagId("Data"));
        Assert.Equal([cv.TagId("Frontend")], cv.Profile.TailorTagIds);

        var snapshot = cv.Profile;
        profiles.SaveTailorTags(cv.ProfileId, new TagLinksRequest { TagIds = [] });
        var restored = profiles.Restore(cv.ProfileId, snapshot).Value!;
        Assert.Equal(["Frontend"], restored.TailorTagIds.Select(id => restored.Tags.Single(t => t.Id == id).Name));
    }
}
