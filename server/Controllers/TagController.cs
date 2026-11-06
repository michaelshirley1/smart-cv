using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagController : ControllerBase
{
    private readonly ITagFactory _tagFactory;

    public TagController(ITagFactory tagFactory)
    {
        _tagFactory = tagFactory;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Tag>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetByProfileId([FromQuery] int profileId)
    {
        return _tagFactory.GetByProfileId(profileId).ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Create([FromBody] CreateTagRequest request)
    {
        return _tagFactory.Create(request).ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, [FromBody] TagRequest request)
    {
        return _tagFactory.Update(id, request).ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        return _tagFactory.Delete(id).ToActionResult();
    }
}
