using SmartCv.Models;
using SmartCv.Repositories;

namespace SmartCv.Factories;

public interface ILineFactory
{
    FactoryResult<ProfileDetail> Create(CreateLineRequest request);
    FactoryResult<ProfileDetail> Update(int id, LineRequest request);
    FactoryResult<ProfileDetail> Delete(int id);
    FactoryResult<ProfileDetail> Reorder(ReorderRequest request);
    FactoryResult<ProfileDetail> SetTags(int id, TagLinksRequest request);
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
        ITagRepository tagRepository)
    {
        _profileFactory = profileFactory;
        _entryRepository = entryRepository;
        _lineRepository = lineRepository;
        _tagRepository = tagRepository;
    }

    public FactoryResult<ProfileDetail> Create(CreateLineRequest request)
    {
        var profileId = _entryRepository.GetProfileId(request.EntryId);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Entry {request.EntryId} not found.");
        if (HasForeignTags(profileId.Value, request.TagIds))
            return FactoryResult<ProfileDetail>.Invalid("Tags must belong to the same profile as the line.");

        var lineId = _lineRepository.Create(new Line
        {
            EntryId = request.EntryId,
            Text = request.Text.Trim(),
            Override = request.Override,
        });
        if (request.TagIds.Count > 0) _lineRepository.SetTags(lineId, request.TagIds);

        return _profileFactory.GetById(profileId.Value);
    }

    public FactoryResult<ProfileDetail> Update(int id, LineRequest request)
    {
        var profileId = _lineRepository.GetProfileId(id);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Line {id} not found.");

        _lineRepository.Update(new Line
        {
            Id = id,
            Text = request.Text.Trim(),
            Override = request.Override,
        });

        return _profileFactory.GetById(profileId.Value);
    }

    public FactoryResult<ProfileDetail> Delete(int id)
    {
        var profileId = _lineRepository.GetProfileId(id);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Line {id} not found.");

        _lineRepository.Delete(id);

        return _profileFactory.GetById(profileId.Value);
    }

    public FactoryResult<ProfileDetail> Reorder(ReorderRequest request)
    {
        var profileId = _entryRepository.GetProfileId(request.ParentId);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Entry {request.ParentId} not found.");

        var existing = _lineRepository.GetByEntryId(request.ParentId).Select(line => line.Id);
        if (!OrderRules.IsCompleteOrder(request.Ids, existing))
            return FactoryResult<ProfileDetail>.Invalid("The new order must list every line in the entry exactly once.");

        _lineRepository.Reorder(request.Ids);

        return _profileFactory.GetById(profileId.Value);
    }

    public FactoryResult<ProfileDetail> SetTags(int id, TagLinksRequest request)
    {
        var profileId = _lineRepository.GetProfileId(id);
        if (profileId is null) return FactoryResult<ProfileDetail>.NotFound($"Line {id} not found.");
        if (HasForeignTags(profileId.Value, request.TagIds))
            return FactoryResult<ProfileDetail>.Invalid("Tags must belong to the same profile as the line.");

        _lineRepository.SetTags(id, request.TagIds);

        return _profileFactory.GetById(profileId.Value);
    }

    private bool HasForeignTags(int profileId, IEnumerable<int> tagIds)
    {
        var profileTags = _tagRepository.GetByProfileId(profileId).Select(tag => tag.Id).ToHashSet();
        return !tagIds.All(profileTags.Contains);
    }
}
