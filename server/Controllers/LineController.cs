using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LineController : ControllerBase
{
    private readonly ILineFactory _lineFactory;

    public LineController(ILineFactory lineFactory)
    {
        _lineFactory = lineFactory;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] CreateLineRequest request)
    {
        return _lineFactory.Create(request).ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, [FromBody] LineRequest request)
    {
        return _lineFactory.Update(id, request).ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        return _lineFactory.Delete(id).ToActionResult();
    }

    [HttpPut("reorder")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Reorder([FromBody] ReorderRequest request)
    {
        return _lineFactory.Reorder(request).ToActionResult();
    }

    [HttpPut("{id:int}/tags")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult SetTags(int id, [FromBody] TagLinksRequest request)
    {
        return _lineFactory.SetTags(id, request).ToActionResult();
    }
}
