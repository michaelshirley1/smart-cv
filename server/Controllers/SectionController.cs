using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SectionController : ControllerBase
{
    private readonly ISectionFactory _sectionFactory;

    public SectionController(ISectionFactory sectionFactory)
    {
        _sectionFactory = sectionFactory;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] CreateSectionRequest request)
    {
        return _sectionFactory.Create(request).ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, [FromBody] SectionRequest request)
    {
        return _sectionFactory.Update(id, request).ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        return _sectionFactory.Delete(id).ToActionResult();
    }

    [HttpPut("reorder")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Reorder([FromBody] ReorderRequest request)
    {
        return _sectionFactory.Reorder(request).ToActionResult();
    }

    [HttpPut("{id:int}/tags")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult SetTags(int id, [FromBody] TagLinksRequest request)
    {
        return _sectionFactory.SetTags(id, request).ToActionResult();
    }
}
