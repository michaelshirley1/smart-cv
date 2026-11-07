using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public static class ProfileBuilder
{
    public static async Task<ProfileDetail> CreateProfile(this TestDatabase database, string name = "Engineering") =>
        (await database.Get<IProfileFactory>().Create(new ProfileRequest { Name = name })).Value!;

    public static async Task<ProfileDetail> AddSection(this TestDatabase database, int profileId, string title = "Work Experience", SectionKind kind = SectionKind.Experience) =>
        (await database.Get<ISectionFactory>().Create(new CreateSectionRequest { ProfileId = profileId, Title = title, Kind = kind })).Value!;

    public static async Task<ProfileDetail> AddLine(this TestDatabase database, int entryId, string text, params int[] tagIds) =>
        (await database.Get<ILineFactory>().Create(new CreateLineRequest { EntryId = entryId, Text = text, TagIds = [.. tagIds] })).Value!;

    public static async Task<ProfileDetail> AddTag(this TestDatabase database, int profileId, string name, params string[] keywords) =>
        (await database.Get<ITagFactory>().Create(new CreateTagRequest { ProfileId = profileId, Name = name, Keywords = [.. keywords] })).Value!;
}
