using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api")]
public class TailorController : ControllerBase
{
    private readonly ITailoringFactory _tailoringFactory;

    public TailorController(ITailoringFactory tailoringFactory)
    {
        _tailoringFactory = tailoringFactory;
    }

    [HttpPost("tailor", Name = "Tailor-Tailor")]
    [ProducesResponseType(typeof(TailorResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Tailor([FromBody] TailorRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tailor = await _tailoringFactory.Tailor(request, cancellationToken);

            return this.ToActionResult(tailor);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
