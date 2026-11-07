using System.ComponentModel.DataAnnotations;

namespace SmartCv.Models;

public class ExportRequest
{
    [Range(1, int.MaxValue)]
    public int ProfileId { get; set; }

    public List<int>? SectionIds { get; set; }

    public List<int>? LineIds { get; set; }
}

public class PrintJob
{
    public string Id { get; set; } = string.Empty;
    public ProfileDetail Profile { get; set; } = new();
    public List<int>? SectionIds { get; set; }
    public List<int>? LineIds { get; set; }
}

public class ExportResult
{
    public bool Saved { get; set; }
    public string? Path { get; set; }
}

public class PageSetup
{
    public string Width { get; set; } = "8.5in";
    public string Height { get; set; } = "11in";
    public string MarginTop { get; set; } = "0.5in";
    public string MarginRight { get; set; } = "0.5in";
    public string MarginBottom { get; set; } = "0.5in";
    public string MarginLeft { get; set; } = "0.5in";
}

public class PdfException(string message, Exception? innerException = null) : Exception(message, innerException);
