using System.Text.RegularExpressions;
using SmartCv.Models;

namespace SmartCv.Factories;

public static partial class KeywordMatcher
{
    public const int MaxCountPerTerm = 3;

    public static string Normalise(string text) => Whitespace().Replace(text.ToLowerInvariant(), " ").Trim();

    public static int CountMatches(string normalisedText, string term)
    {
        var needle = Normalise(term);
        if (needle.Length == 0) return 0;

        var before = char.IsLetterOrDigit(needle[0]) ? @"(?<![\p{L}\p{N}])" : string.Empty;
        var after = char.IsLetterOrDigit(needle[^1]) ? @"(?![\p{L}\p{N}])" : string.Empty;
        return Regex.Matches(normalisedText, before + Regex.Escape(needle) + after).Count;
    }

    public static TagMatch Match(Tag tag, string normalisedText)
    {
        var terms = new[] { tag.Name }
            .Concat(tag.Keywords)
            .Select(Normalise)
            .Where(term => term.Length > 0)
            .Distinct()
            .Select(term => (Term: term, Count: Math.Min(CountMatches(normalisedText, term), MaxCountPerTerm)))
            .Where(match => match.Count > 0)
            .ToList();

        return new TagMatch
        {
            TagId = tag.Id,
            Name = tag.Name,
            Score = terms.Sum(match => match.Count),
            Terms = terms.Select(match => match.Term).ToList(),
        };
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
