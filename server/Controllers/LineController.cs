using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api")]
public class LineController : ControllerBase
{
    private readonly ILineFactory _lineFactory;

    public LineController(ILineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    [HttpPost("line", Name = "Line-Create")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateLineRequest request)
    {
        try
        {
            var create = await _lineFactory.Create(request);

            return this.ToActionResult(create, StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("line/{id:int}", Name = "Line-Update")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] LineRequest request)
    {
        try
        {
            var update = await _lineFactory.Update(id, request);

            return this.ToActionResult(update);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("line/{id:int}", Name = "Line-Delete")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var delete = await _lineFactory.Delete(id);

            return this.ToActionResult(delete);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("line/reorder", Name = "Line-Reorder")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reorder([FromBody] ReorderRequest request)
    {
        try
        {
            var reorder = await _lineFactory.Reorder(request);

            return this.ToActionResult(reorder);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("line/{id:int}/tags", Name = "Line-SetTags")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetTags([FromRoute] int id, [FromBody] TagLinksRequest request)
    {
        try
        {
            var setTags = await _lineFactory.SetTags(id, request);

            return this.ToActionResult(setTags);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
