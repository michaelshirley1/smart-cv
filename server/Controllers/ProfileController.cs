using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileFactory _profileFactory;

    public ProfileController(IProfileFactory profileFactory)
    {
        _profileFactory = profileFactory;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Profile>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        return Ok(_profileFactory.GetAll());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id)
    {
        return _profileFactory.GetById(id).ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] ProfileRequest request)
    {
        return _profileFactory.Create(request).ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, [FromBody] ProfileRequest request)
    {
        return _profileFactory.Update(id, request).ToActionResult();
    }

    [HttpPost("{id:int}/duplicate")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Duplicate(int id)
    {
        return _profileFactory.Duplicate(id).ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Restore(int id, [FromBody] ProfileDetail snapshot)
    {
        return _profileFactory.Restore(id, snapshot).ToActionResult();
    }

    [HttpPut("{id:int}/job-description")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult SaveJobDescription(int id, [FromBody] JobDescriptionRequest request)
    {
        return _profileFactory.SaveJobDescription(id, request).ToActionResult();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        return _profileFactory.Delete(id).ToActionResult();
    }
}
