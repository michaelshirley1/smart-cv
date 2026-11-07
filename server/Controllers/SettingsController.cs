using Microsoft.AspNetCore.Mvc;
using SmartCv.Factories;
using SmartCv.Models;

namespace SmartCv.Controllers;

[ApiController]
[Route("api")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsFactory _settingsFactory;

    public SettingsController(ISettingsFactory settingsFactory)
    {
        _settingsFactory = settingsFactory;
    }

    [HttpGet("settings", Name = "Settings-Fetch")]
    [ProducesResponseType(typeof(AppSettings), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Fetch()
    {
        try
        {
            var fetch = await _settingsFactory.Get();

            return Ok(fetch);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("settings", Name = "Settings-Update")]
    [ProducesResponseType(typeof(AppSettings), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] AppSettings settings)
    {
        try
        {
            var update = await _settingsFactory.Update(settings);

            return Ok(update);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("settings/ollama", Name = "Settings-FetchOllamaStatus")]
    [ProducesResponseType(typeof(OllamaStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FetchOllamaStatus([FromQuery] string? url, CancellationToken cancellationToken)
    {
        try
        {
            var fetchOllamaStatus = await _settingsFactory.GetOllamaStatus(url, cancellationToken);

            return Ok(fetchOllamaStatus);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
