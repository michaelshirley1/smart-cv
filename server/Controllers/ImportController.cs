using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api")]
public class ImportController : ControllerBase
{
    private const long MaxFileSize = 20 * 1024 * 1024;

    private readonly IImportFactory _importFactory;

    public ImportController(IImportFactory importFactory)
    {
        _importFactory = importFactory;
    }

    [HttpPost("import", Name = "Import-Import")]
    [RequestSizeLimit(MaxFileSize)]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import(IFormFile file)
    {
        try
        {
            await using var content = file.OpenReadStream();
            var import = await _importFactory.Import(file.FileName, content);

            return this.ToActionResult(import, StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
