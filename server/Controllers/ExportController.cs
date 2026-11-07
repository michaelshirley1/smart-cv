using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api")]
public class ExportController : ControllerBase
{
    private readonly IExportFactory _exportFactory;

    public ExportController(IExportFactory exportFactory)
    {
        _exportFactory = exportFactory;
    }

    [HttpPost("export/pdf", Name = "Export-Pdf")]
    [ProducesResponseType(typeof(ExportResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportPdf([FromBody] ExportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var exportPdf = await _exportFactory.ExportPdf(request, cancellationToken);

            return this.ToActionResult(exportPdf);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("export/job/{id}", Name = "Export-FetchPrintJob")]
    [ProducesResponseType(typeof(PrintJob), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FetchPrintJob([FromRoute] string id)
    {
        try
        {
            var fetchPrintJob = await _exportFactory.GetPrintJob(id);

            return this.ToActionResult(fetchPrintJob);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
