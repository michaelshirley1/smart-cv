using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Factories;

public interface ILineFactory
{
    Task<FactoryResult<ProfileDetail>> Create(CreateLineRequest request);
    Task<FactoryResult<ProfileDetail>> Update(int id, LineRequest request);
    Task<FactoryResult<ProfileDetail>> Delete(int id);
    Task<FactoryResult<ProfileDetail>> Reorder(ReorderRequest request);
    Task<FactoryResult<ProfileDetail>> SetTags(int id, TagLinksRequest request);
}

public class LineFactory : ILineFactory
{
    private readonly IProfileFactory _profileFactory;
    private readonly IEntryRepository _entryRepository;
    private readonly ILineRepository _lineRepository;
    private readonly ITagRepository _tagRepository;

    public LineFactory(
        IProfileFactory profileFactory,
        IEntryRepository entryRepository,
        ILineRepository lineRepository,
        ITagRepository tagRepository
    )
    {
        _profileFactory = profileFactory;
        _entryRepository = entryRepository;
        _lineRepository = lineRepository;
        _tagRepository = tagRepository;
    }

    public async Task<FactoryResult<ProfileDetail>> Create(CreateLineRequest request)
    {
        var profileId = await _entryRepository.GetProfileId(request.EntryId);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Entry {request.EntryId} not found.");
        if (await HasForeignTags(profileId.Value, request.TagIds))
            return FactoryResult<ProfileDetail>.Invalid("Tags must belong to the same profile as the line.");

        var lineId = await _lineRepository.Create(new Line
        {
            EntryId = request.EntryId,
            Text = request.Text.Trim(),
            Override = request.Override,
            Priority = request.Priority,
        });
        if (request.TagIds.Count > 0) await _lineRepository.SetTags(lineId, request.TagIds);

        return await _profileFactory.GetById(profileId.Value);
    }

    public async Task<FactoryResult<ProfileDetail>> Update(int id, LineRequest request)
    {
        var profileId = await _lineRepository.GetProfileId(id);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Line {id} not found.");

        await _lineRepository.Update(new Line
        {
            Id = id,
            Text = request.Text.Trim(),
            Override = request.Override,
            Priority = request.Priority,
        });

        return await _profileFactory.GetById(profileId.Value);
    }

    public async Task<FactoryResult<ProfileDetail>> Delete(int id)
    {
        var profileId = await _lineRepository.GetProfileId(id);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Line {id} not found.");

        await _lineRepository.Delete(id);

        return await _profileFactory.GetById(profileId.Value);
    }

    public async Task<FactoryResult<ProfileDetail>> Reorder(ReorderRequest request)
    {
        var profileId = await _entryRepository.GetProfileId(request.ParentId);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Entry {request.ParentId} not found.");

        var existing = (await _lineRepository.GetByEntryId(request.ParentId)).Select(line => line.Id);
        if (!OrderRules.IsCompleteOrder(request.Ids, existing))
            return FactoryResult<ProfileDetail>.Invalid("The new order must list every line in the entry exactly once.");

        await _lineRepository.Reorder(request.Ids);

        return await _profileFactory.GetById(profileId.Value);
    }

    public async Task<FactoryResult<ProfileDetail>> SetTags(int id, TagLinksRequest request)
    {
        var profileId = await _lineRepository.GetProfileId(id);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Line {id} not found.");
        if (await HasForeignTags(profileId.Value, request.TagIds))
            return FactoryResult<ProfileDetail>.Invalid("Tags must belong to the same profile as the line.");

        await _lineRepository.SetTags(id, request.TagIds);

        return await _profileFactory.GetById(profileId.Value);
    }

    private async Task<bool> HasForeignTags(int profileId, IEnumerable<int> tagIds)
    {
        var profileTags = (await _tagRepository.GetByProfileId(profileId)).Select(tag => tag.Id).ToHashSet();
        return !tagIds.All(profileTags.Contains);
    }
}
