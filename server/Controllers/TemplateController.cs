using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api")]
public class TemplateController : ControllerBase
{
    private readonly ITemplateFactory _templateFactory;

    public TemplateController(ITemplateFactory templateFactory)
    {
        _templateFactory = templateFactory;
    }

    [HttpGet("template", Name = "Template-FetchAll")]
    [ProducesResponseType(typeof(IEnumerable<TemplateSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FetchAll()
    {
        try
        {
            var fetchAll = await _templateFactory.GetAll();

            return Ok(fetchAll);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("template/{name}", Name = "Template-Fetch")]
    [ProducesResponseType(typeof(Template), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fetch([FromRoute] string name)
    {
        try
        {
            var fetch = await _templateFactory.GetByName(name);

            return this.ToActionResult(fetch);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("template", Name = "Template-Create")]
    [ProducesResponseType(typeof(Template), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] TemplateRequest request)
    {
        try
        {
            var create = await _templateFactory.Create(request);

            return this.ToActionResult(create, StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("template/{name}", Name = "Template-Update")]
    [ProducesResponseType(typeof(Template), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] string name, [FromBody] TemplateRequest request)
    {
        try
        {
            var update = await _templateFactory.Update(name, request);

            return this.ToActionResult(update);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("template/{name}", Name = "Template-Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] string name)
    {
        try
        {
            var delete = await _templateFactory.Delete(name);

            return this.ToActionResult(delete);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
