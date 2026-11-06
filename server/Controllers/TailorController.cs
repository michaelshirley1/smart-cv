using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TailorController : ControllerBase
{
    private readonly ITailoringFactory _tailoringFactory;

    public TailorController(ITailoringFactory tailoringFactory)
    {
        _tailoringFactory = tailoringFactory;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TailorResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Tailor([FromBody] TailorRequest request)
    {
        return _tailoringFactory.Tailor(request).ToActionResult();
    }
}
