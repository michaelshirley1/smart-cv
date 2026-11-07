using SmartCv.Models;
using SmartCv.Services;

namespace SmartCv.Factories;

public interface ITailoringFactory
{
    Task<FactoryResult<TailorResult>> TailorAsync(TailorRequest request, CancellationToken cancellationToken);
}

public class TailoringFactory : ITailoringFactory
{
    public const int RelevantScore = 5;

    private readonly IProfileFactory _profileFactory;
    private readonly ISettingsFactory _settingsFactory;
    private readonly IOllamaService _ollama;
    private readonly IOllamaScoreCache _scoreCache;

    public TailoringFactory(IProfileFactory profileFactory, ISettingsFactory settingsFactory, IOllamaService ollama, IOllamaScoreCache scoreCache)
    {
        _profileFactory = profileFactory;
        _settingsFactory = settingsFactory;
        _ollama = ollama;
        _scoreCache = scoreCache;
    }

    public async Task<FactoryResult<TailorResult>> TailorAsync(TailorRequest request, CancellationToken cancellationToken)
    {
        var profileResult = _profileFactory.GetById(request.ProfileId);
        if (profileResult.Value is null) return FactoryResult<TailorResult>.NotFound(profileResult.Error);

        var profile = profileResult.Value;
        var alwaysOnId = profile.Tags.FirstOrDefault(tag => tag.IsAlwaysOn)?.Id;
        var chosenTags = request.TagIds
            .Distinct()
            .Select(id => profile.Tags.FirstOrDefault(tag => tag.Id == id && !tag.IsAlwaysOn))
            .OfType<Tag>()
            .ToList();

        if (chosenTags.Count == 0 && string.IsNullOrWhiteSpace(request.JobDescription))
            return FactoryResult<TailorResult>.Invalid("Paste a job description or choose some tags.");

        var byTags = chosenTags.Count > 0;
        var matches = byTags ? WeightByOrder(chosenTags) : MatchKeywords(profile, KeywordMatcher.Normalise(request.JobDescription));
        var tagScores = matches.ToDictionary(match => match.TagId, match => match.Score);

        var result = new TailorResult
        {
            Source = byTags ? TailorSource.Tags : TailorSource.Description,
            Mode = MatchingMode.Keyword,
            MatchedTags = matches,
        };
        var modelScores = byTags ? null : await ModelScoresAsync(profile, request.JobDescription, alwaysOnId, result, cancellationToken);

        foreach (var section in profile.Sections)
        {
            var decision = DecideSection(section, alwaysOnId, tagScores, modelScores);
            var included = decision is SectionDecision.AlwaysOn or SectionDecision.Untagged or SectionDecision.Matched;
            result.Sections.Add(new SectionResult { SectionId = section.Id, Included = included, Decision = decision });

            foreach (var entry in section.Entries)
            {
                result.Lines.AddRange(DecideLines(section, entry, included, decision == SectionDecision.AlwaysOn, alwaysOnId, tagScores, modelScores));
            }
        }

        result.TrimOrder = TrimOrder(result.Lines, profile.Sections.ToDictionary(section => section.Id, section => section.Priority));
        return FactoryResult<TailorResult>.Success(result);
    }

    private static List<TagMatch> WeightByOrder(IReadOnlyList<Tag> tags) =>
        tags.Select((tag, index) => new TagMatch { TagId = tag.Id, Name = tag.Name, Score = tags.Count - index }).ToList();

    private static List<TagMatch> MatchKeywords(ProfileDetail profile, string text) =>
        profile.Tags
            .Where(tag => !tag.IsAlwaysOn)
            .Select(tag => KeywordMatcher.Match(tag, text))
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private async Task<Dictionary<int, int>?> ModelScoresAsync(ProfileDetail profile, string jobDescription, int? alwaysOnId, TailorResult result, CancellationToken cancellationToken)
    {
        var settings = _settingsFactory.Get();
        if (settings.MatchingMode != MatchingMode.Ollama) return null;

        if (string.IsNullOrWhiteSpace(settings.OllamaModel))
        {
            result.Notice = "No Ollama model is chosen in Settings, so keyword matching was used.";
            return null;
        }

        var tagNames = profile.Tags.Where(tag => !tag.IsAlwaysOn).ToDictionary(tag => tag.Id, tag => tag.Name);
        var lines = profile.Sections
            .Where(section => !section.IsHidden && !(alwaysOnId is not null && section.TagIds.Contains(alwaysOnId.Value)))
            .SelectMany(section => section.Entries.SelectMany(entry => entry.Lines
                .Where(line => line.Override != LineOverride.Hidden && !(alwaysOnId is not null && line.TagIds.Contains(alwaysOnId.Value)))
                .Select(line => new OllamaLine
                {
                    Id = line.Id,
                    Context = string.Join(" · ", new[] { section.Title, entry.Heading }.Where(part => part.Length > 0)),
                    Text = line.Text.Replace("**", string.Empty),
                    Tags = line.TagIds.Where(tagNames.ContainsKey).Select(id => tagNames[id]).ToList(),
                })))
            .ToList();

        var scope = _scoreCache.Scope(settings.OllamaModel, jobDescription);
        var scores = new Dictionary<int, int>();
        var missing = new List<OllamaLine>();
        foreach (var line in lines)
        {
            if (_scoreCache.TryGet(scope, CacheKey(line), out var cached)) scores[line.Id] = cached;
            else missing.Add(line);
        }

        try
        {
            var fresh = await _ollama.ScoreAsync(settings.OllamaUrl, settings.OllamaModel, jobDescription, missing, cancellationToken);
            foreach (var line in missing)
            {
                var score = fresh.GetValueOrDefault(line.Id);
                scores[line.Id] = score;
                _scoreCache.Set(scope, CacheKey(line), score);
            }
        }
        catch (OllamaException ex)
        {
            result.Notice = $"{ex.Message} Keyword matching was used instead.";
            return null;
        }

        result.Mode = MatchingMode.Ollama;
        result.Model = settings.OllamaModel;
        return scores;
    }

    private static string CacheKey(OllamaLine line) => $"{line.Context}\n{line.Text}\n{string.Join(',', line.Tags)}";

    private static SectionDecision DecideSection(Section section, int? alwaysOnId, IReadOnlyDictionary<int, int> tagScores, IReadOnlyDictionary<int, int>? modelScores)
    {
        if (section.IsHidden) return SectionDecision.Hidden;
        if (alwaysOnId is not null && section.TagIds.Contains(alwaysOnId.Value)) return SectionDecision.AlwaysOn;

        var tags = section.TagIds.Where(id => id != alwaysOnId).ToList();
        if (tags.Count == 0) return SectionDecision.Untagged;
        if (tags.Any(tagScores.ContainsKey)) return SectionDecision.Matched;

        var relevantLine = modelScores is not null && section.Entries
            .SelectMany(entry => entry.Lines)
            .Any(line => modelScores.GetValueOrDefault(line.Id) >= RelevantScore);
        return relevantLine ? SectionDecision.Matched : SectionDecision.Unmatched;
    }

    private static List<LineResult> DecideLines(
        Section section,
        Entry entry,
        bool sectionIncluded,
        bool sectionAlwaysOn,
        int? alwaysOnId,
        IReadOnlyDictionary<int, int> tagScores,
        IReadOnlyDictionary<int, int>? modelScores)
    {
        var lines = entry.Lines.Select(line =>
        {
            var tags = line.TagIds.Where(id => id != alwaysOnId).ToList();
            var score = modelScores is null ? tags.Sum(id => tagScores.GetValueOrDefault(id)) : modelScores.GetValueOrDefault(line.Id);
            var relevant = modelScores is null ? score > 0 : score >= RelevantScore;
            var decision = line.Override == LineOverride.Hidden ? LineDecision.Hidden
                : !sectionIncluded ? LineDecision.SectionExcluded
                : sectionAlwaysOn || (alwaysOnId is not null && line.TagIds.Contains(alwaysOnId.Value)) ? LineDecision.AlwaysOn
                : relevant ? LineDecision.Matched
                : modelScores is null && tags.Count == 0 ? LineDecision.Untagged
                : LineDecision.Unmatched;

            return new LineResult
            {
                LineId = line.Id,
                EntryId = entry.Id,
                SectionId = section.Id,
                Score = score,
                Priority = line.Priority,
                Decision = decision,
                Included = decision == LineDecision.AlwaysOn,
            };
        }).ToList();

        if (!sectionIncluded) return lines;

        var slots = Math.Max(0, section.MaxLines - lines.Count(line => line.Decision == LineDecision.AlwaysOn));
        var candidates = lines.Where(line => line.Decision == LineDecision.Matched)
            .OrderByDescending(line => line.Priority)
            .ThenByDescending(line => line.Score)
            .ThenBy(lines.IndexOf)
            .Concat(lines.Where(line => line.Decision == LineDecision.Untagged)
                .OrderByDescending(line => line.Priority)
                .ThenBy(lines.IndexOf))
            .ToList();

        foreach (var (line, index) in candidates.Select((line, index) => (line, index)))
        {
            if (index < slots) line.Included = true;
            else line.Decision = LineDecision.OverLimit;
        }

        if (!lines.Any(line => line.Included))
        {
            var fallback = lines
                .Where(line => line.Decision is LineDecision.Unmatched or LineDecision.OverLimit)
                .OrderByDescending(line => line.Priority)
                .ThenByDescending(line => line.Score)
                .ThenBy(lines.IndexOf)
                .FirstOrDefault();
            if (fallback is not null)
            {
                fallback.Included = true;
                fallback.Decision = LineDecision.Fallback;
            }
        }

        return lines;
    }

    private static List<int> TrimOrder(List<LineResult> lines, IReadOnlyDictionary<int, int> sectionPriorities)
    {
        var position = lines.Select((line, index) => (line.LineId, index)).ToDictionary(pair => pair.LineId, pair => pair.index);
        var candidates = lines
            .Where(line => line.Included && line.Decision != LineDecision.AlwaysOn)
            .OrderBy(line => sectionPriorities[line.SectionId])
            .ThenBy(line => line.Priority)
            .ThenBy(line => line.Decision == LineDecision.Matched)
            .ThenBy(line => line.Decision == LineDecision.Matched ? line.Score : 0)
            .ThenByDescending(line => position[line.LineId]);

        var remaining = lines.Where(line => line.Included).GroupBy(line => line.EntryId).ToDictionary(group => group.Key, group => group.Count());
        var order = new List<int>();
        foreach (var line in candidates)
        {
            if (remaining[line.EntryId] <= 1) continue;
            remaining[line.EntryId]--;
            order.Add(line.LineId);
        }

        return order;
    }
}
