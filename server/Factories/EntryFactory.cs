using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Factories;

public interface IEntryFactory
{
    Task<FactoryResult<ProfileDetail>> Create(CreateEntryRequest request);
    Task<FactoryResult<ProfileDetail>> Update(int id, EntryRequest request);
    Task<FactoryResult<ProfileDetail>> Delete(int id);
    Task<FactoryResult<ProfileDetail>> Reorder(ReorderRequest request);
}

public class EntryFactory : IEntryFactory
{
    private readonly IProfileFactory _profileFactory;
    private readonly ISectionRepository _sectionRepository;
    private readonly IEntryRepository _entryRepository;

    public EntryFactory(
        IProfileFactory profileFactory,
        ISectionRepository sectionRepository,
        IEntryRepository entryRepository
    )
    {
        _profileFactory = profileFactory;
        _sectionRepository = sectionRepository;
        _entryRepository = entryRepository;
    }

    public async Task<FactoryResult<ProfileDetail>> Create(CreateEntryRequest request)
    {
        var section = await _sectionRepository.GetById(request.SectionId);
        if (section is null) return FactoryResult<ProfileDetail>.NotFound($"Section {request.SectionId} not found.");

        var entry = ToEntry(request);
        entry.SectionId = section.Id;
        await _entryRepository.Create(entry);

        return await _profileFactory.GetById(section.ProfileId);
    }

    public async Task<FactoryResult<ProfileDetail>> Update(int id, EntryRequest request)
    {
        var profileId = await _entryRepository.GetProfileId(id);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Entry {id} not found.");

        var entry = ToEntry(request);
        entry.Id = id;
        await _entryRepository.Update(entry);

        return await _profileFactory.GetById(profileId.Value);
    }

    public async Task<FactoryResult<ProfileDetail>> Delete(int id)
    {
        var profileId = await _entryRepository.GetProfileId(id);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Entry {id} not found.");

        await _entryRepository.Delete(id);

        return await _profileFactory.GetById(profileId.Value);
    }

    public async Task<FactoryResult<ProfileDetail>> Reorder(ReorderRequest request)
    {
        var section = await _sectionRepository.GetById(request.ParentId);
        if (section is null) return FactoryResult<ProfileDetail>.NotFound($"Section {request.ParentId} not found.");

        var existing = (await _entryRepository.GetBySectionId(section.Id)).Select(entry => entry.Id);
        if (!OrderRules.IsCompleteOrder(request.Ids, existing))
            return FactoryResult<ProfileDetail>.Invalid("The new order must list every entry in the section exactly once.");

        await _entryRepository.Reorder(request.Ids);

        return await _profileFactory.GetById(section.ProfileId);
    }

    private static Entry ToEntry(EntryRequest request) => new()
    {
        Heading = request.Heading.Trim(),
        Subheading = request.Subheading.Trim(),
        Location = request.Location.Trim(),
        DateRange = request.DateRange.Trim(),
    };
}
