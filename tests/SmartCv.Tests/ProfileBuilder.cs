using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public static class ProfileBuilder
{
    public static ProfileDetail CreateProfile(this TestDatabase database, string name = "Engineering") =>
        database.Get<IProfileFactory>().Create(new ProfileRequest { Name = name }).Value!;

    public static ProfileDetail AddSection(this TestDatabase database, int profileId, string title = "Work Experience", SectionKind kind = SectionKind.Experience) =>
        database.Get<ISectionFactory>().Create(new CreateSectionRequest { ProfileId = profileId, Title = title, Kind = kind }).Value!;

    public static ProfileDetail AddLine(this TestDatabase database, int entryId, string text, params int[] tagIds) =>
        database.Get<ILineFactory>().Create(new CreateLineRequest { EntryId = entryId, Text = text, TagIds = [.. tagIds] }).Value!;

    public static ProfileDetail AddTag(this TestDatabase database, int profileId, string name, params string[] keywords) =>
        database.Get<ITagFactory>().Create(new CreateTagRequest { ProfileId = profileId, Name = name, Keywords = [.. keywords] }).Value!;
}
