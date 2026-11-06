using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Factories;

public interface ITagFactory
{
    FactoryResult<List<Tag>> GetByProfileId(int profileId);
    FactoryResult<ProfileDetail> Create(CreateTagRequest request);
    FactoryResult<ProfileDetail> Update(int id, TagRequest request);
    FactoryResult<ProfileDetail> Delete(int id);
}

public class TagFactory : ITagFactory
{
    private readonly IProfileFactory _profileFactory;
    private readonly IProfileRepository _profileRepository;
    private readonly ITagRepository _tagRepository;

    public TagFactory(IProfileFactory profileFactory, IProfileRepository profileRepository, ITagRepository tagRepository)
    {
        _profileFactory = profileFactory;
        _profileRepository = profileRepository;
        _tagRepository = tagRepository;
    }

    public FactoryResult<List<Tag>> GetByProfileId(int profileId)
    {
        if (_profileRepository.GetById(profileId) is null)
            return FactoryResult<List<Tag>>.NotFound($"Profile {profileId} not found.");

        return FactoryResult<List<Tag>>.Success(_tagRepository.GetByProfileId(profileId).ToList());
    }

    public FactoryResult<ProfileDetail> Create(CreateTagRequest request)
    {
        if (_profileRepository.GetById(request.ProfileId) is null)
            return FactoryResult<ProfileDetail>.NotFound($"Profile {request.ProfileId} not found.");

        var name = request.Name.Trim();
        if (IsNameTaken(request.ProfileId, name, null))
            return FactoryResult<ProfileDetail>.Invalid($"A tag called {name} already exists.");

        _tagRepository.Create(new Tag
        {
            ProfileId = request.ProfileId,
            Name = name,
            Keywords = NormaliseKeywords(request.Keywords),
            Colour = request.Colour.Trim(),
        });

        return _profileFactory.GetById(request.ProfileId);
    }

    public FactoryResult<ProfileDetail> Update(int id, TagRequest request)
    {
        var tag = _tagRepository.GetById(id);
        if (tag is null) return FactoryResult<ProfileDetail>.NotFound($"Tag {id} not found.");

        var name = request.Name.Trim();
        if (tag.IsAlwaysOn && name != tag.Name)
            return FactoryResult<ProfileDetail>.Invalid($"The {Tag.AlwaysOnName} tag can't be renamed.");
        if (IsNameTaken(tag.ProfileId, name, id))
            return FactoryResult<ProfileDetail>.Invalid($"A tag called {name} already exists.");

        _tagRepository.Update(new Tag
        {
            Id = id,
            Name = name,
            Keywords = NormaliseKeywords(request.Keywords),
            Colour = request.Colour.Trim(),
        });

        return _profileFactory.GetById(tag.ProfileId);
    }

    public FactoryResult<ProfileDetail> Delete(int id)
    {
        var tag = _tagRepository.GetById(id);
        if (tag is null) return FactoryResult<ProfileDetail>.NotFound($"Tag {id} not found.");
        if (tag.IsAlwaysOn) return FactoryResult<ProfileDetail>.Invalid($"The {Tag.AlwaysOnName} tag can't be deleted.");

        _tagRepository.Delete(id);

        return _profileFactory.GetById(tag.ProfileId);
    }

    private bool IsNameTaken(int profileId, string name, int? excludeId) =>
        _tagRepository.GetByProfileId(profileId)
            .Any(tag => tag.Id != excludeId && string.Equals(tag.Name, name, StringComparison.OrdinalIgnoreCase));

    private static List<string> NormaliseKeywords(IEnumerable<string> keywords) =>
        keywords
            .Select(keyword => keyword.Trim().ToLowerInvariant())
            .Where(keyword => keyword.Length > 0)
            .Distinct()
            .ToList();
}
