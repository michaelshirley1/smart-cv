namespace SmartCv.Models;

public class OllamaStatus
{
    public bool Reachable { get; set; }
    public List<string> Models { get; set; } = [];
    public string? Error { get; set; }
}

public class OllamaLine
{
    public int Id { get; set; }
    public string Context { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = [];
}

public class OllamaException(string message, Exception? innerException = null) : Exception(message, innerException);
