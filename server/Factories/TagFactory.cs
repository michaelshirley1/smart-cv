using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Factories;

public interface ITagFactory
{
    Task<FactoryResult<List<Tag>>> GetByProfileId(int profileId);
    Task<FactoryResult<ProfileDetail>> Create(CreateTagRequest request);
    Task<FactoryResult<ProfileDetail>> Update(int id, TagRequest request);
    Task<FactoryResult<ProfileDetail>> Delete(int id);
}

public class TagFactory : ITagFactory
{
    private readonly IProfileFactory _profileFactory;
    private readonly IProfileRepository _profileRepository;
    private readonly ITagRepository _tagRepository;

    public TagFactory(
        IProfileFactory profileFactory,
        IProfileRepository profileRepository,
        ITagRepository tagRepository
    )
    {
        _profileFactory = profileFactory;
        _profileRepository = profileRepository;
        _tagRepository = tagRepository;
    }

    public async Task<FactoryResult<List<Tag>>> GetByProfileId(int profileId)
    {
        if (await _profileRepository.GetById(profileId) is null)
            return FactoryResult<List<Tag>>.NotFound($"Profile {profileId} not found.");

        return FactoryResult<List<Tag>>.Success((await _tagRepository.GetByProfileId(profileId)).ToList());
    }

    public async Task<FactoryResult<ProfileDetail>> Create(CreateTagRequest request)
    {
        if (await _profileRepository.GetById(request.ProfileId) is null)
            return FactoryResult<ProfileDetail>.NotFound($"Profile {request.ProfileId} not found.");

        var name = request.Name.Trim();
        if (await IsNameTaken(request.ProfileId, name, null))
            return FactoryResult<ProfileDetail>.Invalid($"A tag called {name} already exists.");

        await _tagRepository.Create(new Tag
        {
            ProfileId = request.ProfileId,
            Name = name,
            Keywords = NormaliseKeywords(request.Keywords),
            Colour = request.Colour.Trim(),
        });

        return await _profileFactory.GetById(request.ProfileId);
    }

    public async Task<FactoryResult<ProfileDetail>> Update(int id, TagRequest request)
    {
        var tag = await _tagRepository.GetById(id);
        if (tag is null) return FactoryResult<ProfileDetail>.NotFound($"Tag {id} not found.");

        var name = request.Name.Trim();
        if (tag.IsAlwaysOn && name != tag.Name)
            return FactoryResult<ProfileDetail>.Invalid($"The {Tag.AlwaysOnName} tag can't be renamed.");
        if (await IsNameTaken(tag.ProfileId, name, id))
            return FactoryResult<ProfileDetail>.Invalid($"A tag called {name} already exists.");

        await _tagRepository.Update(new Tag
        {
            Id = id,
            Name = name,
            Keywords = NormaliseKeywords(request.Keywords),
            Colour = request.Colour.Trim(),
        });

        return await _profileFactory.GetById(tag.ProfileId);
    }

    public async Task<FactoryResult<ProfileDetail>> Delete(int id)
    {
        var tag = await _tagRepository.GetById(id);
        if (tag is null) return FactoryResult<ProfileDetail>.NotFound($"Tag {id} not found.");
        if (tag.IsAlwaysOn) return FactoryResult<ProfileDetail>.Invalid($"The {Tag.AlwaysOnName} tag can't be deleted.");

        await _tagRepository.Delete(id);

        return await _profileFactory.GetById(tag.ProfileId);
    }

    private async Task<bool> IsNameTaken(int profileId, string name, int? excludeId) =>
        (await _tagRepository.GetByProfileId(profileId))
            .Any(tag => tag.Id != excludeId && string.Equals(tag.Name, name, StringComparison.OrdinalIgnoreCase));

    private static List<string> NormaliseKeywords(IEnumerable<string> keywords) =>
        keywords
            .Select(keyword => keyword.Trim().ToLowerInvariant())
            .Where(keyword => keyword.Length > 0)
            .Distinct()
            .ToList();
}
