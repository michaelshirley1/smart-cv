using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Factories;

public interface ISectionFactory
{
    FactoryResult<ProfileDetail> Create(CreateSectionRequest request);
    FactoryResult<ProfileDetail> Update(int id, SectionRequest request);
    FactoryResult<ProfileDetail> Delete(int id);
    FactoryResult<ProfileDetail> Reorder(ReorderRequest request);
    FactoryResult<ProfileDetail> SetTags(int id, TagLinksRequest request);
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
        ITagRepository tagRepository)
    {
        _profileFactory = profileFactory;
        _profileRepository = profileRepository;
        _sectionRepository = sectionRepository;
        _entryRepository = entryRepository;
        _tagRepository = tagRepository;
    }

    public FactoryResult<ProfileDetail> Create(CreateSectionRequest request)
    {
        if (_profileRepository.GetById(request.ProfileId) is null)
            return FactoryResult<ProfileDetail>.NotFound($"Profile {request.ProfileId} not found.");

        var sectionId = _sectionRepository.Create(new Section
        {
            ProfileId = request.ProfileId,
            Title = request.Title.Trim(),
            Kind = request.Kind,
            MaxLines = request.MaxLines,
            IsHidden = request.IsHidden,
            Priority = request.Priority,
        });
        _entryRepository.Create(new Entry { SectionId = sectionId });

        return _profileFactory.GetById(request.ProfileId);
    }

    public FactoryResult<ProfileDetail> Update(int id, SectionRequest request)
    {
        var section = _sectionRepository.GetById(id);
        if (section is null) return FactoryResult<ProfileDetail>.NotFound($"Section {id} not found.");

        _sectionRepository.Update(new Section
        {
            Id = id,
            Title = request.Title.Trim(),
            Kind = request.Kind,
            MaxLines = request.MaxLines,
            IsHidden = request.IsHidden,
            Priority = request.Priority,
        });

        return _profileFactory.GetById(section.ProfileId);
    }

    public FactoryResult<ProfileDetail> Delete(int id)
    {
        var section = _sectionRepository.GetById(id);
        if (section is null) return FactoryResult<ProfileDetail>.NotFound($"Section {id} not found.");

        _sectionRepository.Delete(id);

        return _profileFactory.GetById(section.ProfileId);
    }

    public FactoryResult<ProfileDetail> Reorder(ReorderRequest request)
    {
        if (_profileRepository.GetById(request.ParentId) is null)
            return FactoryResult<ProfileDetail>.NotFound($"Profile {request.ParentId} not found.");

        var existing = _sectionRepository.GetByProfileId(request.ParentId).Select(section => section.Id);
        if (!OrderRules.IsCompleteOrder(request.Ids, existing))
            return FactoryResult<ProfileDetail>.Invalid("The new order must list every section in the profile exactly once.");

        _sectionRepository.Reorder(request.Ids);

        return _profileFactory.GetById(request.ParentId);
    }

    public FactoryResult<ProfileDetail> SetTags(int id, TagLinksRequest request)
    {
        var section = _sectionRepository.GetById(id);
        if (section is null) return FactoryResult<ProfileDetail>.NotFound($"Section {id} not found.");

        var profileTags = _tagRepository.GetByProfileId(section.ProfileId).Select(tag => tag.Id).ToHashSet();
        if (!request.TagIds.All(profileTags.Contains))
            return FactoryResult<ProfileDetail>.Invalid("Tags must belong to the same profile as the section.");

        _sectionRepository.SetTags(id, request.TagIds);

        return _profileFactory.GetById(section.ProfileId);
    }
}
