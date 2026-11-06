using SmartCv.Models;
using SmartCv.Repositories;
using SmartCv.Services;

namespace SmartCv.Factories;

public interface IProfileFactory
{
    IEnumerable<Profile> GetAll();
    FactoryResult<ProfileDetail> GetById(int id);
    FactoryResult<ProfileDetail> Create(ProfileRequest request);
    FactoryResult<ProfileDetail> Update(int id, ProfileRequest request);
    FactoryResult<ProfileDetail> Duplicate(int id);
    FactoryResult Delete(int id);
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
        ITemplateFileService templateFiles)
    {
        _profileRepository = profileRepository;
        _tagRepository = tagRepository;
        _sectionRepository = sectionRepository;
        _entryRepository = entryRepository;
        _lineRepository = lineRepository;
        _templateFiles = templateFiles;
    }

    public IEnumerable<Profile> GetAll() => _profileRepository.GetAll();

    public FactoryResult<ProfileDetail> GetById(int id)
    {
        var profile = _profileRepository.GetById(id);
        if (profile is null) return FactoryResult<ProfileDetail>.NotFound($"Profile {id} not found.");

        var lineTags = _lineRepository.GetTagLinks(id).ToLookup(link => link.OwnerId, link => link.TagId);
        var sectionTags = _sectionRepository.GetTagLinks(id).ToLookup(link => link.OwnerId, link => link.TagId);

        var lines = _lineRepository.GetByProfileId(id).ToList();
        lines.ForEach(line => line.TagIds = lineTags[line.Id].ToList());
        var linesByEntry = lines.ToLookup(line => line.EntryId);

        var entries = _entryRepository.GetByProfileId(id).ToList();
        entries.ForEach(entry => entry.Lines = linesByEntry[entry.Id].ToList());
        var entriesBySection = entries.ToLookup(entry => entry.SectionId);

        var sections = _sectionRepository.GetByProfileId(id).ToList();
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
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt,
            Tags = _tagRepository.GetByProfileId(id).ToList(),
            Sections = sections,
        });
    }

    public FactoryResult<ProfileDetail> Create(ProfileRequest request)
    {
        var templateName = string.IsNullOrWhiteSpace(request.TemplateName) ? TemplateSummary.DefaultName : request.TemplateName.Trim();
        if (!_templateFiles.Exists(templateName)) return FactoryResult<ProfileDetail>.Invalid($"Template {templateName} does not exist.");

        var id = _profileRepository.Create(ToProfile(request, templateName));
        _tagRepository.Create(new Tag { ProfileId = id, Name = Tag.AlwaysOnName, IsAlwaysOn = true });

        return GetById(id);
    }

    public FactoryResult<ProfileDetail> Update(int id, ProfileRequest request)
    {
        var existing = _profileRepository.GetById(id);
        if (existing is null) return FactoryResult<ProfileDetail>.NotFound($"Profile {id} not found.");

        var templateName = string.IsNullOrWhiteSpace(request.TemplateName) ? existing.TemplateName : request.TemplateName.Trim();
        if (!_templateFiles.Exists(templateName)) return FactoryResult<ProfileDetail>.Invalid($"Template {templateName} does not exist.");

        var profile = ToProfile(request, templateName);
        profile.Id = id;
        _profileRepository.Update(profile);

        return GetById(id);
    }

    public FactoryResult<ProfileDetail> Duplicate(int id)
    {
        var source = GetById(id);
        if (source.Value is null) return source;

        var original = source.Value;
        var copyId = _profileRepository.Create(new Profile
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

        var tagIds = original.Tags.ToDictionary(
            tag => tag.Id,
            tag => _tagRepository.Create(new Tag
            {
                ProfileId = copyId,
                Name = tag.Name,
                Keywords = tag.Keywords,
                Colour = tag.Colour,
                IsAlwaysOn = tag.IsAlwaysOn,
            }));

        foreach (var section in original.Sections)
        {
            var sectionId = _sectionRepository.Create(new Section
            {
                ProfileId = copyId,
                Title = section.Title,
                Kind = section.Kind,
                MaxLines = section.MaxLines,
                IsHidden = section.IsHidden,
            });
            _sectionRepository.SetTags(sectionId, section.TagIds.Select(tagId => tagIds[tagId]));

            foreach (var entry in section.Entries)
            {
                var entryId = _entryRepository.Create(new Entry
                {
                    SectionId = sectionId,
                    Heading = entry.Heading,
                    Subheading = entry.Subheading,
                    Location = entry.Location,
                    DateRange = entry.DateRange,
                });

                foreach (var line in entry.Lines)
                {
                    var lineId = _lineRepository.Create(new Line
                    {
                        EntryId = entryId,
                        Text = line.Text,
                        Override = line.Override,
                    });
                    _lineRepository.SetTags(lineId, line.TagIds.Select(tagId => tagIds[tagId]));
                }
            }
        }

        return GetById(copyId);
    }

    public FactoryResult Delete(int id)
    {
        return _profileRepository.Delete(id) > 0
            ? FactoryResult.Success()
            : FactoryResult.NotFound($"Profile {id} not found.");
    }

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
