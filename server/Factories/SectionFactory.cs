using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Factories;

public interface ISectionFactory
{
    Task<FactoryResult<ProfileDetail>> Create(CreateSectionRequest request);
    Task<FactoryResult<ProfileDetail>> Update(int id, SectionRequest request);
    Task<FactoryResult<ProfileDetail>> Delete(int id);
    Task<FactoryResult<ProfileDetail>> Reorder(ReorderRequest request);
    Task<FactoryResult<ProfileDetail>> SetTags(int id, TagLinksRequest request);
}

public class SectionFactory : ISectionFactory
{
    private readonly IProfileFactory _profileFactory;
    private readonly IProfileRepository _profileRepository;
    private readonly ISectionRepository _sectionRepository;
    private readonly IEntryRepository _entryRepository;
    private readonly ITagRepository _tagRepository;

    public SectionFactory(
        IProfileFactory profileFactory,
        IProfileRepository profileRepository,
        ISectionRepository sectionRepository,
        IEntryRepository entryRepository,
        ITagRepository tagRepository
    )
    {
        _profileFactory = profileFactory;
        _profileRepository = profileRepository;
        _sectionRepository = sectionRepository;
        _entryRepository = entryRepository;
        _tagRepository = tagRepository;
    }

    public async Task<FactoryResult<ProfileDetail>> Create(CreateSectionRequest request)
    {
        if (await _profileRepository.GetById(request.ProfileId) is null)
            return FactoryResult<ProfileDetail>.NotFound($"Profile {request.ProfileId} not found.");

        var sectionId = await _sectionRepository.Create(new Section
        {
            ProfileId = request.ProfileId,
            Title = request.Title.Trim(),
            Kind = request.Kind,
            MaxLines = request.MaxLines,
            IsHidden = request.IsHidden,
            Priority = request.Priority,
            MaxTotal = request.MaxTotal,
        });
        await _entryRepository.Create(new Entry { SectionId = sectionId });

        return await _profileFactory.GetById(request.ProfileId);
    }

    public async Task<FactoryResult<ProfileDetail>> Update(int id, SectionRequest request)
    {
        var section = await _sectionRepository.GetById(id);
        if (section is null) return FactoryResult<ProfileDetail>.NotFound($"Section {id} not found.");

        await _sectionRepository.Update(new Section
        {
            Id = id,
            Title = request.Title.Trim(),
            Kind = request.Kind,
            MaxLines = request.MaxLines,
            IsHidden = request.IsHidden,
            Priority = request.Priority,
            MaxTotal = request.MaxTotal,
        });

        return await _profileFactory.GetById(section.ProfileId);
    }

    public async Task<FactoryResult<ProfileDetail>> Delete(int id)
    {
        var section = await _sectionRepository.GetById(id);
        if (section is null) return FactoryResult<ProfileDetail>.NotFound($"Section {id} not found.");

        await _sectionRepository.Delete(id);

        return await _profileFactory.GetById(section.ProfileId);
    }

    public async Task<FactoryResult<ProfileDetail>> Reorder(ReorderRequest request)
    {
        if (await _profileRepository.GetById(request.ParentId) is null)
            return FactoryResult<ProfileDetail>.NotFound($"Profile {request.ParentId} not found.");

        var existing = (await _sectionRepository.GetByProfileId(request.ParentId)).Select(section => section.Id);
        if (!OrderRules.IsCompleteOrder(request.Ids, existing))
            return FactoryResult<ProfileDetail>.Invalid("The new order must list every section in the profile exactly once.");

        await _sectionRepository.Reorder(request.Ids);

        return await _profileFactory.GetById(request.ParentId);
    }

    public async Task<FactoryResult<ProfileDetail>> SetTags(int id, TagLinksRequest request)
    {
        var section = await _sectionRepository.GetById(id);
        if (section is null) return FactoryResult<ProfileDetail>.NotFound($"Section {id} not found.");

        var profileTags = (await _tagRepository.GetByProfileId(section.ProfileId)).Select(tag => tag.Id).ToHashSet();
        if (!request.TagIds.All(profileTags.Contains))
            return FactoryResult<ProfileDetail>.Invalid("Tags must belong to the same profile as the section.");

        await _sectionRepository.SetTags(id, request.TagIds);

        return await _profileFactory.GetById(section.ProfileId);
    }
}
