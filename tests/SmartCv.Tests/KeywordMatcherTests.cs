using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Tests;

public class KeywordMatcherTests
{
    private static int Count(string text, string term) => KeywordMatcher.CountMatches(KeywordMatcher.Normalise(text), term);

    [Theory]
    [InlineData("We use React and TypeScript", "react", 1)]
    [InlineData("Experience with ReactJS", "react", 0)]
    [InlineData("Strong C# skills, C#/.NET preferred", "C#", 2)]
    [InlineData("Built on ASP.NET Core and .NET 8", ".net", 2)]
    [InlineData("Machine\n   Learning engineers wanted", "machine learning", 1)]
    [InlineData("Node.js services", "node.js", 1)]
    [InlineData("Go, Rust, or Python", "go", 1)]
    [InlineData("Google Cloud", "go", 0)]
    [InlineData("Anything", "   ", 0)]
    public void CountMatches_RespectsWordEdges(string text, string term, int expected)
    {
        Assert.Equal(expected, Count(text, term));
    }

    [Fact]
    public void Match_SumsDistinctTermsAndCapsRepeats()
    {
        var tag = new Tag { Id = 1, Name = "Backend", Keywords = ["c#", "backend", "SQL"] };
        var text = KeywordMatcher.Normalise("Backend backend backend backend role. C# and SQL. SQL!");

        var match = KeywordMatcher.Match(tag, text);

        Assert.Equal(KeywordMatcher.MaxCountPerTerm + 1 + 2, match.Score);
        Assert.Equal(["backend", "c#", "sql"], match.Terms);
    }
}
