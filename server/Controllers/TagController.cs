using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api")]
public class TagController : ControllerBase
{
    private readonly ITagFactory _tagFactory;

    public TagController(ITagFactory tagFactory)
    {
        _tagFactory = tagFactory;
    }

    [HttpGet("tag", Name = "Tag-FetchByProfile")]
    [ProducesResponseType(typeof(List<Tag>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FetchByProfile([FromQuery] int profileId)
    {
        try
        {
            var fetchByProfile = await _tagFactory.GetByProfileId(profileId);

            return this.ToActionResult(fetchByProfile);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("tag", Name = "Tag-Create")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest request)
    {
        try
        {
            var create = await _tagFactory.Create(request);

            return this.ToActionResult(create, StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("tag/{id:int}", Name = "Tag-Update")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TagRequest request)
    {
        try
        {
            var update = await _tagFactory.Update(id, request);

            return this.ToActionResult(update);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("tag/{id:int}", Name = "Tag-Delete")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var delete = await _tagFactory.Delete(id);

            return this.ToActionResult(delete);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
