using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace SmartCv.Services;

public interface IOllamaScoreCache
{
    string Scope(string model, string jobDescription);
    bool TryGet(string scope, string lineKey, out int score);
    void Set(string scope, string lineKey, int score);
}

public class OllamaScoreCache : IOllamaScoreCache
{
    private const int MaxEntries = 20000;

    private readonly ConcurrentDictionary<string, int> _scores = new();

    public string Scope(string model, string jobDescription) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{model}\n{jobDescription}")));

    public bool TryGet(string scope, string lineKey, out int score) =>
        _scores.TryGetValue(Key(scope, lineKey), out score);

    public void Set(string scope, string lineKey, int score)
    {
        if (_scores.Count >= MaxEntries) _scores.Clear();
        _scores[Key(scope, lineKey)] = score;
    }

    private static string Key(string scope, string lineKey) => $"{scope}\n{lineKey}";
}
