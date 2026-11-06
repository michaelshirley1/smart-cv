using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntryController : ControllerBase
{
    private readonly IEntryFactory _entryFactory;

    public EntryController(IEntryFactory entryFactory)
    {
        _entryFactory = entryFactory;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] CreateEntryRequest request)
    {
        return _entryFactory.Create(request).ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, [FromBody] EntryRequest request)
    {
        return _entryFactory.Update(id, request).ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        return _entryFactory.Delete(id).ToActionResult();
    }

    [HttpPut("reorder")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Reorder([FromBody] ReorderRequest request)
    {
        return _entryFactory.Reorder(request).ToActionResult();
    }
}
