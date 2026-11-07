using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartCv.Models;

namespace SmartCv.Services;

public interface IOllamaService
{
    Task<OllamaStatus> GetStatusAsync(string baseUrl, CancellationToken cancellationToken);
    Task<Dictionary<int, int>> ScoreAsync(string baseUrl, string model, string jobDescription, IReadOnlyList<OllamaLine> lines, CancellationToken cancellationToken);
}

public class OllamaService : IOllamaService
{
    public const int MaxScore = 10;

    private static readonly TimeSpan StatusTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan ScoreTimeout = TimeSpan.FromMinutes(3);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly object ScoreSchema = new
    {
        type = "object",
        properties = new
        {
            scores = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        id = new { type = "integer" },
                        score = new { type = "integer", minimum = 0, maximum = MaxScore },
                    },
                    required = new[] { "id", "score" },
                },
            },
        },
        required = new[] { "scores" },
    };

    private readonly HttpClient _http;

    public OllamaService(HttpClient http)
    {
        _http = http;
        _http.Timeout = Timeout.InfiniteTimeSpan;
    }

    public async Task<OllamaStatus> GetStatusAsync(string baseUrl, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(StatusTimeout);

        try
        {
            var tags = await _http.GetFromJsonAsync<TagsResponse>(Endpoint(baseUrl, "api/tags"), Json, timeout.Token);
            return new OllamaStatus
            {
                Reachable = true,
                Models = tags?.Models.Select(model => model.Name).Order().ToList() ?? [],
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or UriFormatException)
        {
            return new OllamaStatus { Reachable = false, Error = $"Ollama isn't answering at {baseUrl}." };
        }
    }

    public async Task<Dictionary<int, int>> ScoreAsync(string baseUrl, string model, string jobDescription, IReadOnlyList<OllamaLine> lines, CancellationToken cancellationToken)
    {
        if (lines.Count == 0) return [];

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(ScoreTimeout);

        var request = new
        {
            model,
            prompt = Prompt(jobDescription, lines),
            stream = false,
            format = ScoreSchema,
            options = new { temperature = 0 },
        };

        GenerateResponse? response;
        try
        {
            using var http = await _http.PostAsJsonAsync(Endpoint(baseUrl, "api/generate"), request, Json, timeout.Token);
            if (!http.IsSuccessStatusCode)
                throw new OllamaException($"Ollama returned {(int)http.StatusCode} for model {model}. Check the model is installed.");
            response = await http.Content.ReadFromJsonAsync<GenerateResponse>(Json, timeout.Token);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new OllamaException("Ollama took too long to answer.", ex);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or UriFormatException)
        {
            throw new OllamaException($"Ollama isn't answering at {baseUrl}.", ex);
        }

        return Parse(response?.Response, lines);
    }

    private static Dictionary<int, int> Parse(string? content, IReadOnlyList<OllamaLine> lines)
    {
        ScoreResponse? parsed;
        try
        {
            parsed = string.IsNullOrWhiteSpace(content) ? null : JsonSerializer.Deserialize<ScoreResponse>(content, Json);
        }
        catch (JsonException ex)
        {
            throw new OllamaException("Ollama's answer wasn't valid JSON.", ex);
        }

        var known = lines.Select(line => line.Id).ToHashSet();
        var scores = (parsed?.Scores ?? [])
            .Where(score => known.Contains(score.Id))
            .GroupBy(score => score.Id)
            .ToDictionary(group => group.Key, group => Math.Clamp(group.Last().Score, 0, MaxScore));

        if (scores.Count * 2 < lines.Count)
            throw new OllamaException("Ollama didn't score enough of your lines.");

        return lines.ToDictionary(line => line.Id, line => scores.GetValueOrDefault(line.Id));
    }

    private static string Prompt(string jobDescription, IReadOnlyList<OllamaLine> lines) => $"""
        You are helping someone tailor their CV to a job advert.
        Score how relevant each CV line is to the job, from 0 to {MaxScore}.
        {MaxScore} means it shows experience the job directly asks for. 5 means related but not central. 0 means irrelevant.
        Judge by meaning, not by shared words alone. Score every line by its id.

        Job advert:
        <<<
        {jobDescription}
        >>>

        CV lines:
        {JsonSerializer.Serialize(lines, Json)}
        """;

    private static Uri Endpoint(string baseUrl, string path) => new(new Uri(baseUrl.TrimEnd('/') + "/"), path);

    private sealed class TagsResponse
    {
        public List<ModelInfo> Models { get; set; } = [];
    }

    private sealed class ModelInfo
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class GenerateResponse
    {
        public string? Response { get; set; }
    }

    private sealed class ScoreResponse
    {
        public List<LineScore> Scores { get; set; } = [];
    }

    private sealed class LineScore
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }
    }
}
