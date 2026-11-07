using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api")]
public class ProfileController : ControllerBase
{
    private readonly IProfileFactory _profileFactory;

    public ProfileController(IProfileFactory profileFactory)
    {
        _profileFactory = profileFactory;
    }

    [HttpGet("profile", Name = "Profile-FetchAll")]
    [ProducesResponseType(typeof(IEnumerable<Profile>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FetchAll()
    {
        try
        {
            var fetchAll = await _profileFactory.GetAll();

            return Ok(fetchAll);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("profile/{id:int}", Name = "Profile-Fetch")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fetch([FromRoute] int id)
    {
        try
        {
            var fetch = await _profileFactory.GetById(id);

            return this.ToActionResult(fetch);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("profile", Name = "Profile-Create")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProfileRequest request)
    {
        try
        {
            var create = await _profileFactory.Create(request);

            return this.ToActionResult(create, StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("profile/{id:int}", Name = "Profile-Update")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProfileRequest request)
    {
        try
        {
            var update = await _profileFactory.Update(id, request);

            return this.ToActionResult(update);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("profile/{id:int}/duplicate", Name = "Profile-Duplicate")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Duplicate([FromRoute] int id)
    {
        try
        {
            var duplicate = await _profileFactory.Duplicate(id);

            return this.ToActionResult(duplicate, StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("profile/{id:int}/restore", Name = "Profile-Restore")]
    [ProducesResponseType(typeof(ProfileDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore([FromRoute] int id, [FromBody] ProfileDetail snapshot)
    {
        try
        {
            var restore = await _profileFactory.Restore(id, snapshot);

            return this.ToActionResult(restore);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("profile/{id:int}/job-description", Name = "Profile-SaveJobDescription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveJobDescription([FromRoute] int id, [FromBody] JobDescriptionRequest request)
    {
        try
        {
            var saveJobDescription = await _profileFactory.SaveJobDescription(id, request);

            return this.ToActionResult(saveJobDescription);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("profile/{id:int}/tailor-tags", Name = "Profile-SaveTailorTags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveTailorTags([FromRoute] int id, [FromBody] TagLinksRequest request)
    {
        try
        {
            var saveTailorTags = await _profileFactory.SaveTailorTags(id, request);

            return this.ToActionResult(saveTailorTags);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("profile/{id:int}", Name = "Profile-Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var delete = await _profileFactory.Delete(id);

            return this.ToActionResult(delete);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
