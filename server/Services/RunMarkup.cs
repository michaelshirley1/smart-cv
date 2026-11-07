using System.Text;
using System.Text.RegularExpressions;

namespace SmartCv.Services;

public static partial class RunMarkup
{
    public static string Build(IEnumerable<(string Text, bool Bold)> runs)
    {
        var merged = new List<(string Text, bool Bold)>();
        foreach (var run in runs.Where(run => run.Text.Length > 0))
        {
            if (merged.Count > 0 && (merged[^1].Bold == run.Bold || string.IsNullOrWhiteSpace(run.Text)))
                merged[^1] = (merged[^1].Text + run.Text, merged[^1].Bold);
            else
                merged.Add(run);
        }

        var text = new StringBuilder();
        foreach (var (content, bold) in merged)
        {
            var trimmed = content.Trim();
            if (!bold || trimmed.Length == 0)
            {
                text.Append(content);
                continue;
            }

            var leading = content[..(content.Length - content.TrimStart().Length)];
            var trailing = content[content.TrimEnd().Length..];
            text.Append(leading).Append("**").Append(trimmed).Append("**").Append(trailing);
        }

        return Whitespace().Replace(text.ToString(), " ").Trim();
    }

    public static bool IsBold(IEnumerable<(string Text, bool Bold)> runs)
    {
        var visible = runs.Where(run => !string.IsNullOrWhiteSpace(run.Text)).ToList();
        return visible.Count > 0 && visible.All(run => run.Bold);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
