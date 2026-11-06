using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;

namespace SmartCv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FontController : ControllerBase
{
    private readonly IFontFactory _fontFactory;

    public FontController(IFontFactory fontFactory)
    {
        _fontFactory = fontFactory;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        return Ok(_fontFactory.GetAll());
    }
}
