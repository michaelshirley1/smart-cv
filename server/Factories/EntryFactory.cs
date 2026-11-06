using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Factories;

public interface IEntryFactory
{
    FactoryResult<ProfileDetail> Create(CreateEntryRequest request);
    FactoryResult<ProfileDetail> Update(int id, EntryRequest request);
    FactoryResult<ProfileDetail> Delete(int id);
    FactoryResult<ProfileDetail> Reorder(ReorderRequest request);
}

public class EntryFactory : IEntryFactory
{
    private readonly IProfileFactory _profileFactory;
    private readonly ISectionRepository _sectionRepository;
    private readonly IEntryRepository _entryRepository;

    public EntryFactory(IProfileFactory profileFactory, ISectionRepository sectionRepository, IEntryRepository entryRepository)
    {
        _profileFactory = profileFactory;
        _sectionRepository = sectionRepository;
        _entryRepository = entryRepository;
    }

    public FactoryResult<ProfileDetail> Create(CreateEntryRequest request)
    {
        var section = _sectionRepository.GetById(request.SectionId);
        if (section is null) return FactoryResult<ProfileDetail>.NotFound($"Section {request.SectionId} not found.");

        var entry = ToEntry(request);
        entry.SectionId = section.Id;
        _entryRepository.Create(entry);

        return _profileFactory.GetById(section.ProfileId);
    }

    public FactoryResult<ProfileDetail> Update(int id, EntryRequest request)
    {
        var profileId = _entryRepository.GetProfileId(id);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Entry {id} not found.");

        var entry = ToEntry(request);
        entry.Id = id;
        _entryRepository.Update(entry);

        return _profileFactory.GetById(profileId.Value);
    }

    public FactoryResult<ProfileDetail> Delete(int id)
    {
        var profileId = _entryRepository.GetProfileId(id);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Entry {id} not found.");

        _entryRepository.Delete(id);

        return _profileFactory.GetById(profileId.Value);
    }

    public FactoryResult<ProfileDetail> Reorder(ReorderRequest request)
    {
        var section = _sectionRepository.GetById(request.ParentId);
        if (section is null) return FactoryResult<ProfileDetail>.NotFound($"Section {request.ParentId} not found.");

        var existing = _entryRepository.GetBySectionId(section.Id).Select(entry => entry.Id);
        if (!OrderRules.IsCompleteOrder(request.Ids, existing))
            return FactoryResult<ProfileDetail>.Invalid("The new order must list every entry in the section exactly once.");

        _entryRepository.Reorder(request.Ids);

        return _profileFactory.GetById(section.ProfileId);
    }

    private static Entry ToEntry(EntryRequest request) => new()
    {
        Heading = request.Heading.Trim(),
        Subheading = request.Subheading.Trim(),
        Location = request.Location.Trim(),
        DateRange = request.DateRange.Trim(),
    };
}
