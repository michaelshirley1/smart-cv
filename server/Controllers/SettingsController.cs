using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsFactory _settingsFactory;

    public SettingsController(ISettingsFactory settingsFactory)
    {
        _settingsFactory = settingsFactory;
    }

    [HttpGet]
    [ProducesResponseType(typeof(AppSettings), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(_settingsFactory.Get());
    }

    [HttpPut]
    [ProducesResponseType(typeof(AppSettings), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Update([FromBody] AppSettings settings)
    {
        return Ok(_settingsFactory.Update(settings));
    }

    [HttpGet("ollama")]
    [ProducesResponseType(typeof(OllamaStatus), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOllamaStatus([FromQuery] string? url, CancellationToken cancellationToken)
    {
        return Ok(await _settingsFactory.GetOllamaStatusAsync(url, cancellationToken));
    }
}
