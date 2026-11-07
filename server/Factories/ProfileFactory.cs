using SmartCv.Models;
using SmartCv.Repositories;
using SmartCv.Services;

namespace SmartCv.Factories;

public interface IProfileFactory
{
    Task<IEnumerable<Profile>> GetAll();
    Task<FactoryResult<ProfileDetail>> GetById(int id);
    Task<FactoryResult<ProfileDetail>> Create(ProfileRequest request);
    Task<FactoryResult<ProfileDetail>> Update(int id, ProfileRequest request);
    Task<FactoryResult<ProfileDetail>> Duplicate(int id);
    Task<FactoryResult<ProfileDetail>> Restore(int id, ProfileDetail snapshot);
    Task<FactoryResult> SaveJobDescription(int id, JobDescriptionRequest request);
    Task<FactoryResult> SaveTailorTags(int id, TagLinksRequest request);
    Task<FactoryResult> Delete(int id);
}

public class ProfileFactory : IProfileFactory
{
    private readonly IProfileRepository _profileRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ISectionRepository _sectionRepository;
    private readonly IEntryRepository _entryRepository;
    private readonly ILineRepository _lineRepository;
    private readonly ITemplateFileService _templateFiles;

    public ProfileFactory(
        IProfileRepository profileRepository,
        ITagRepository tagRepository,
        ISectionRepository sectionRepository,
        IEntryRepository entryRepository,
        ILineRepository lineRepository,
        ITemplateFileService templateFiles
    )
    {
        _profileRepository = profileRepository;
        _tagRepository = tagRepository;
        _sectionRepository = sectionRepository;
        _entryRepository = entryRepository;
        _lineRepository = lineRepository;
        _templateFiles = templateFiles;
    }

    public async Task<IEnumerable<Profile>> GetAll() => await _profileRepository.GetAll();

    public async Task<FactoryResult<ProfileDetail>> GetById(int id)
    {
        var profile = await _profileRepository.GetById(id);
        if (profile is null) return FactoryResult<ProfileDetail>.NotFound($"Profile {id} not found.");

        var lineTags = (await _lineRepository.GetTagLinks(id)).ToLookup(link => link.OwnerId, link => link.TagId);
        var sectionTags = (await _sectionRepository.GetTagLinks(id)).ToLookup(link => link.OwnerId, link => link.TagId);

        var lines = (await _lineRepository.GetByProfileId(id)).ToList();
        lines.ForEach(line => line.TagIds = lineTags[line.Id].ToList());
        var linesByEntry = lines.ToLookup(line => line.EntryId);

        var entries = (await _entryRepository.GetByProfileId(id)).ToList();
        entries.ForEach(entry => entry.Lines = linesByEntry[entry.Id].ToList());
        var entriesBySection = entries.ToLookup(entry => entry.SectionId);

        var sections = (await _sectionRepository.GetByProfileId(id)).ToList();
        sections.ForEach(section =>
        {
            section.TagIds = sectionTags[section.Id].ToList();
            section.Entries = entriesBySection[section.Id].ToList();
        });

        return FactoryResult<ProfileDetail>.Success(new ProfileDetail
        {
            Id = profile.Id,
            Name = profile.Name,
            TemplateName = profile.TemplateName,
            TargetPages = profile.TargetPages,
            FullName = profile.FullName,
            Headline = profile.Headline,
            Email = profile.Email,
            Phone = profile.Phone,
            Location = profile.Location,
            Links = profile.Links,
            JobDescription = profile.JobDescription,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt,
            Tags = (await _tagRepository.GetByProfileId(id)).ToList(),
            Sections = sections,
            TailorTagIds = (await _tagRepository.GetTailorTagIds(id)).ToList(),
        });
    }

    public async Task<FactoryResult<ProfileDetail>> Create(ProfileRequest request)
    {
        var templateName = string.IsNullOrWhiteSpace(request.TemplateName) ? TemplateSummary.DefaultName : request.TemplateName.Trim();
        if (!_templateFiles.Exists(templateName)) return FactoryResult<ProfileDetail>.Invalid($"Template {templateName} does not exist.");

        var id = await _profileRepository.Create(ToProfile(request, templateName));
        await _tagRepository.Create(new Tag { ProfileId = id, Name = Tag.AlwaysOnName, IsAlwaysOn = true });

        return await GetById(id);
    }

    public async Task<FactoryResult<ProfileDetail>> Update(int id, ProfileRequest request)
    {
        var existing = await _profileRepository.GetById(id);
        if (existing is null) return FactoryResult<ProfileDetail>.NotFound($"Profile {id} not found.");

        var templateName = string.IsNullOrWhiteSpace(request.TemplateName) ? existing.TemplateName : request.TemplateName.Trim();
        if (!_templateFiles.Exists(templateName)) return FactoryResult<ProfileDetail>.Invalid($"Template {templateName} does not exist.");

        var profile = ToProfile(request, templateName);
        profile.Id = id;
        await _profileRepository.Update(profile);

        return await GetById(id);
    }

    public async Task<FactoryResult<ProfileDetail>> Duplicate(int id)
    {
        var source = await GetById(id);
        if (source.Value is null) return source;

        var original = source.Value;
        var copyId = await _profileRepository.Create(new Profile
        {
            Name = $"{original.Name} (copy)",
            TemplateName = original.TemplateName,
            TargetPages = original.TargetPages,
            FullName = original.FullName,
            Headline = original.Headline,
            Email = original.Email,
            Phone = original.Phone,
            Location = original.Location,
            Links = original.Links,
        });
        await _profileRepository.SaveJobDescription(copyId, original.JobDescription);

        var tagIds = new Dictionary<int, int>();
        foreach (var tag in original.Tags) tagIds[tag.Id] = await _tagRepository.Create(CopyOf(tag, copyId));
        await CopySections(original.Sections, copyId, tagIds);
        await _tagRepository.SetTailorTags(copyId, original.TailorTagIds.Where(tagIds.ContainsKey).Select(tagId => tagIds[tagId]).ToList());

        return await GetById(copyId);
    }

    public async Task<FactoryResult<ProfileDetail>> Restore(int id, ProfileDetail snapshot)
    {
        var existing = await GetById(id);
        if (existing.Value is null) return existing;

        var templateName = _templateFiles.Exists(snapshot.TemplateName) ? snapshot.TemplateName : existing.Value.TemplateName;
        await _profileRepository.Update(new Profile
        {
            Id = id,
            Name = snapshot.Name,
            TemplateName = templateName,
            TargetPages = snapshot.TargetPages,
            FullName = snapshot.FullName,
            Headline = snapshot.Headline,
            Email = snapshot.Email,
            Phone = snapshot.Phone,
            Location = snapshot.Location,
            Links = snapshot.Links,
        });
        await _profileRepository.SaveJobDescription(id, snapshot.JobDescription);

        foreach (var section in existing.Value.Sections) await _sectionRepository.Delete(section.Id);
        foreach (var tag in existing.Value.Tags.Where(tag => !tag.IsAlwaysOn)) await _tagRepository.Delete(tag.Id);

        var alwaysOn = existing.Value.Tags.First(tag => tag.IsAlwaysOn);
        var tagIds = new Dictionary<int, int>();
        foreach (var tag in snapshot.Tags)
            tagIds[tag.Id] = tag.IsAlwaysOn ? alwaysOn.Id : await _tagRepository.Create(CopyOf(tag, id));
        await CopySections(snapshot.Sections, id, tagIds);
        await _tagRepository.SetTailorTags(id, snapshot.TailorTagIds.Where(tagIds.ContainsKey).Select(tagId => tagIds[tagId]).ToList());

        return await GetById(id);
    }

    public async Task<FactoryResult> SaveJobDescription(int id, JobDescriptionRequest request)
    {
        return await _profileRepository.SaveJobDescription(id, request.JobDescription.Trim()) > 0
            ? FactoryResult.Success()
            : FactoryResult.NotFound($"Profile {id} not found.");
    }

    public async Task<FactoryResult> SaveTailorTags(int id, TagLinksRequest request)
    {
        if (await _profileRepository.GetById(id) is null) return FactoryResult.NotFound($"Profile {id} not found.");

        var allowed = (await _tagRepository.GetByProfileId(id)).Where(tag => !tag.IsAlwaysOn).Select(tag => tag.Id).ToHashSet();
        var tagIds = request.TagIds.Distinct().ToList();
        if (!tagIds.All(allowed.Contains)) return FactoryResult.Invalid("Choose tags from this profile, other than Always on.");

        await _tagRepository.SetTailorTags(id, tagIds);
        return FactoryResult.Success();
    }

    public async Task<FactoryResult> Delete(int id)
    {
        return await _profileRepository.Delete(id) > 0
            ? FactoryResult.Success()
            : FactoryResult.NotFound($"Profile {id} not found.");
    }

    private async Task CopySections(IEnumerable<Section> sections, int profileId, IReadOnlyDictionary<int, int> tagIds)
    {
        IEnumerable<int> Mapped(IEnumerable<int> ids) => ids.Where(tagIds.ContainsKey).Select(tagId => tagIds[tagId]);

        foreach (var section in sections)
        {
            var sectionId = await _sectionRepository.Create(new Section
            {
                ProfileId = profileId,
                Title = section.Title,
                Kind = section.Kind,
                MaxLines = section.MaxLines,
                IsHidden = section.IsHidden,
                Priority = section.Priority,
                MaxTotal = section.MaxTotal,
            });
            await _sectionRepository.SetTags(sectionId, Mapped(section.TagIds));

            foreach (var entry in section.Entries)
            {
                var entryId = await _entryRepository.Create(new Entry
                {
                    SectionId = sectionId,
                    Heading = entry.Heading,
                    Subheading = entry.Subheading,
                    Location = entry.Location,
                    DateRange = entry.DateRange,
                });

                foreach (var line in entry.Lines)
                {
                    var lineId = await _lineRepository.Create(new Line
                    {
                        EntryId = entryId,
                        Text = line.Text,
                        Override = line.Override,
                        Priority = line.Priority,
                    });
                    await _lineRepository.SetTags(lineId, Mapped(line.TagIds));
                }
            }
        }
    }

    private static Tag CopyOf(Tag tag, int profileId) => new()
    {
        ProfileId = profileId,
        Name = tag.Name,
        Keywords = tag.Keywords,
        Colour = tag.Colour,
        IsAlwaysOn = tag.IsAlwaysOn,
    };

    private static Profile ToProfile(ProfileRequest request, string templateName) => new()
    {
        Name = request.Name.Trim(),
        TemplateName = templateName,
        TargetPages = request.TargetPages,
        FullName = request.FullName.Trim(),
        Headline = request.Headline.Trim(),
        Email = request.Email.Trim(),
        Phone = request.Phone.Trim(),
        Location = request.Location.Trim(),
        Links = request.Links.Select(link => link.Trim()).Where(link => link.Length > 0).ToList(),
    };
}
