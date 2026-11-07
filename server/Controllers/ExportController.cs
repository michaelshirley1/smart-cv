using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IExportFactory _exportFactory;

    public ExportController(IExportFactory exportFactory)
    {
        _exportFactory = exportFactory;
    }

    [HttpPost("pdf")]
    [ProducesResponseType(typeof(ExportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportPdf([FromBody] ExportRequest request, CancellationToken cancellationToken)
    {
        return (await _exportFactory.ExportPdfAsync(request, cancellationToken)).ToActionResult();
    }

    [HttpGet("job/{id}")]
    [ProducesResponseType(typeof(PrintJob), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetPrintJob(string id)
    {
        return _exportFactory.GetPrintJob(id).ToActionResult();
    }
}
