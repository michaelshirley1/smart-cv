using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;

namespace SmartCv.Controllers;

[ApiController]
[Route("api")]
public class FontController : ControllerBase
{
    private readonly IFontFactory _fontFactory;

    public FontController(IFontFactory fontFactory)
    {
        _fontFactory = fontFactory;
    }

    [HttpGet("font", Name = "Font-FetchAll")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FetchAll()
    {
        try
        {
            var fetchAll = await _fontFactory.GetAll();

            return Ok(fetchAll);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
