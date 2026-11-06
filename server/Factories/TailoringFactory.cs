using SmartCv.Models;

namespace SmartCv.Factories;

public interface ITailoringFactory
{
    FactoryResult<TailorResult> Tailor(TailorRequest request);
}

public class TailoringFactory : ITailoringFactory
{
    private readonly IProfileFactory _profileFactory;

    public TailoringFactory(IProfileFactory profileFactory)
    {
        _profileFactory = profileFactory;
    }

    public FactoryResult<TailorResult> Tailor(TailorRequest request)
    {
        var profileResult = _profileFactory.GetById(request.ProfileId);
        if (profileResult.Value is null) return FactoryResult<TailorResult>.NotFound(profileResult.Error);

        var profile = profileResult.Value;
        var text = KeywordMatcher.Normalise(request.JobDescription);
        var alwaysOnId = profile.Tags.FirstOrDefault(tag => tag.IsAlwaysOn)?.Id;

        var matches = profile.Tags
            .Where(tag => !tag.IsAlwaysOn)
            .Select(tag => KeywordMatcher.Match(tag, text))
            .Where(match => match.Score > 0)
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var tagScores = matches.ToDictionary(match => match.TagId, match => match.Score);

        var result = new TailorResult { Mode = MatchingMode.Keyword, MatchedTags = matches };

        foreach (var section in profile.Sections)
        {
            var decision = DecideSection(section, alwaysOnId, tagScores);
            var included = decision is SectionDecision.AlwaysOn or SectionDecision.Untagged or SectionDecision.Matched;
            result.Sections.Add(new SectionResult { SectionId = section.Id, Included = included, Decision = decision });

            foreach (var entry in section.Entries)
            {
                result.Lines.AddRange(DecideLines(section, entry, included, decision == SectionDecision.AlwaysOn, alwaysOnId, tagScores));
            }
        }

        result.TrimOrder = TrimOrder(result.Lines);
        return FactoryResult<TailorResult>.Success(result);
    }

    private static SectionDecision DecideSection(Section section, int? alwaysOnId, IReadOnlyDictionary<int, int> tagScores)
    {
        if (section.IsHidden) return SectionDecision.Hidden;
        if (alwaysOnId is not null && section.TagIds.Contains(alwaysOnId.Value)) return SectionDecision.AlwaysOn;

        var tags = section.TagIds.Where(id => id != alwaysOnId).ToList();
        if (tags.Count == 0) return SectionDecision.Untagged;
        return tags.Any(tagScores.ContainsKey) ? SectionDecision.Matched : SectionDecision.Unmatched;
    }

    private static List<LineResult> DecideLines(Section section, Entry entry, bool sectionIncluded, bool sectionAlwaysOn, int? alwaysOnId, IReadOnlyDictionary<int, int> tagScores)
    {
        var lines = entry.Lines.Select(line =>
        {
            var tags = line.TagIds.Where(id => id != alwaysOnId).ToList();
            var score = tags.Sum(id => tagScores.GetValueOrDefault(id));
            var decision = line.Override == LineOverride.Hidden ? LineDecision.Hidden
                : !sectionIncluded ? LineDecision.SectionExcluded
                : sectionAlwaysOn || (alwaysOnId is not null && line.TagIds.Contains(alwaysOnId.Value)) ? LineDecision.AlwaysOn
                : score > 0 ? LineDecision.Matched
                : tags.Count == 0 ? LineDecision.Untagged
                : LineDecision.Unmatched;

            return new LineResult
            {
                LineId = line.Id,
                EntryId = entry.Id,
                SectionId = section.Id,
                Score = score,
                Decision = decision,
                Included = decision == LineDecision.AlwaysOn,
            };
        }).ToList();

        if (!sectionIncluded) return lines;

        var slots = Math.Max(0, section.MaxLines - lines.Count(line => line.Decision == LineDecision.AlwaysOn));
        var candidates = lines.Where(line => line.Decision == LineDecision.Matched)
            .OrderByDescending(line => line.Score)
            .ThenBy(lines.IndexOf)
            .Concat(lines.Where(line => line.Decision == LineDecision.Untagged))
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
                .OrderByDescending(line => line.Score)
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

    private static List<int> TrimOrder(List<LineResult> lines)
    {
        var position = lines.Select((line, index) => (line.LineId, index)).ToDictionary(pair => pair.LineId, pair => pair.index);
        var trimmable = lines.Where(line => line.Included && line.Decision != LineDecision.AlwaysOn).ToList();

        var generic = trimmable
            .Where(line => line.Decision is LineDecision.Untagged or LineDecision.Fallback)
            .OrderByDescending(line => position[line.LineId]);
        var matched = trimmable
            .Where(line => line.Decision == LineDecision.Matched)
            .OrderBy(line => line.Score)
            .ThenByDescending(line => position[line.LineId]);

        var remaining = lines.Where(line => line.Included).GroupBy(line => line.EntryId).ToDictionary(group => group.Key, group => group.Count());
        var order = new List<int>();
        foreach (var line in generic.Concat(matched))
        {
            if (remaining[line.EntryId] <= 1) continue;
            remaining[line.EntryId]--;
            order.Add(line.LineId);
        }

        return order;
    }
}
