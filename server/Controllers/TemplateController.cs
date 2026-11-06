using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplateController : ControllerBase
{
    private readonly ITemplateFactory _templateFactory;

    public TemplateController(ITemplateFactory templateFactory)
    {
        _templateFactory = templateFactory;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TemplateSummary>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        return Ok(_templateFactory.GetAll());
    }

    [HttpGet("{name}")]
    [ProducesResponseType(typeof(Template), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetByName(string name)
    {
        return _templateFactory.GetByName(name).ToActionResult();
    }

    [HttpPost]
    [ProducesResponseType(typeof(Template), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] TemplateRequest request)
    {
        return _templateFactory.Create(request).ToActionResult(StatusCodes.Status201Created);
    }

    [HttpPut("{name}")]
    [ProducesResponseType(typeof(Template), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Update(string name, [FromBody] TemplateRequest request)
    {
        return _templateFactory.Update(name, request).ToActionResult();
    }

    [HttpDelete("{name}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Delete(string name)
    {
        return _templateFactory.Delete(name).ToActionResult();
    }
}
