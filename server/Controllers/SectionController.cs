using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api")]
public class SectionController : ControllerBase
{
    private readonly ISectionFactory _sectionFactory;

    public SectionController(ISectionFactory sectionFactory)
    {
        _sectionFactory = sectionFactory;
    }

    [HttpPost("section", Name = "Section-Create")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateSectionRequest request)
    {
        try
        {
            var create = await _sectionFactory.Create(request);

            return this.ToActionResult(create, StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("section/{id:int}", Name = "Section-Update")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SectionRequest request)
    {
        try
        {
            var update = await _sectionFactory.Update(id, request);

            return this.ToActionResult(update);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("section/{id:int}", Name = "Section-Delete")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var delete = await _sectionFactory.Delete(id);

            return this.ToActionResult(delete);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("section/reorder", Name = "Section-Reorder")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reorder([FromBody] ReorderRequest request)
    {
        try
        {
            var reorder = await _sectionFactory.Reorder(request);

            return this.ToActionResult(reorder);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("section/{id:int}/tags", Name = "Section-SetTags")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetTags([FromRoute] int id, [FromBody] TagLinksRequest request)
    {
        try
        {
            var setTags = await _sectionFactory.SetTags(id, request);

            return this.ToActionResult(setTags);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
