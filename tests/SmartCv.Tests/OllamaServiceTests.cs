using System.Net;
using System.Text;
using System.Text.Json;
using SmartCv.Models;
using SmartCv.Services;

namespace SmartCv.Tests;

public class OllamaServiceTests
{
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content is not null) Bodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
            return respond(request);
        }
    }

    private static HttpResponseMessage Json(object body) =>
        new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") };

    private static HttpResponseMessage Generated(string response) => Json(new { model = "llama3.2", response, done = true });

    private static readonly OllamaLine[] Lines =
    [
        new() { Id = 1, Context = "Experience", Text = "React work" },
        new() { Id = 2, Context = "Experience", Text = "SQL work" },
    ];

    [Fact]
    public async Task GetStatus_ListsModels()
    {
        var service = new OllamaService(new HttpClient(new StubHandler(_ => Json(new { models = new[] { new { name = "qwen2.5:7b" }, new { name = "llama3.2" } } }))));

        var status = await service.GetStatusAsync("http://localhost:11434", CancellationToken.None);

        Assert.True(status.Reachable);
        Assert.Equal(["llama3.2", "qwen2.5:7b"], status.Models);
    }

    [Fact]
    public async Task GetStatus_ReportsUnreachable()
    {
        var service = new OllamaService(new HttpClient(new StubHandler(_ => throw new HttpRequestException("refused"))));

        var status = await service.GetStatusAsync("http://localhost:11434", CancellationToken.None);

        Assert.False(status.Reachable);
        Assert.Equal("Ollama isn't answering at http://localhost:11434.", status.Error);
    }

    [Fact]
    public async Task Score_SendsASchemaAndClampsScores()
    {
        var handler = new StubHandler(_ => Generated("""{"scores":[{"id":1,"score":14},{"id":2,"score":3},{"id":99,"score":10}]}"""));
        var service = new OllamaService(new HttpClient(handler));

        var scores = await service.ScoreAsync("http://localhost:11434/", "llama3.2", "React role", Lines, CancellationToken.None);

        Assert.Equal(new Dictionary<int, int> { [1] = 10, [2] = 3 }, scores);
        using var body = JsonDocument.Parse(handler.Bodies.Single());
        Assert.Equal("llama3.2", body.RootElement.GetProperty("model").GetString());
        Assert.False(body.RootElement.GetProperty("stream").GetBoolean());
        Assert.Equal("object", body.RootElement.GetProperty("format").GetProperty("type").GetString());
        Assert.Contains("React role", body.RootElement.GetProperty("prompt").GetString());
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("""{"scores":[]}""")]
    public async Task Score_RejectsUnusableAnswers(string response)
    {
        var service = new OllamaService(new HttpClient(new StubHandler(_ => Generated(response))));

        await Assert.ThrowsAsync<OllamaException>(() => service.ScoreAsync("http://localhost:11434", "llama3.2", "x", Lines, CancellationToken.None));
    }

    [Fact]
    public async Task Score_ExplainsAMissingModel()
    {
        var service = new OllamaService(new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound))));

        var error = await Assert.ThrowsAsync<OllamaException>(() => service.ScoreAsync("http://localhost:11434", "missing", "x", Lines, CancellationToken.None));

        Assert.Contains("Check the model is installed", error.Message);
    }
}
