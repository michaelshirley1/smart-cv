using Microsoft.AspNetCore.Mvc;
using SmartCv.Api.Models;
using SmartCv.Api.Repositories;

namespace SmartCv.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResumeController(IResumeRepository resumeRepository) : ControllerBase
{
    private readonly IResumeRepository _resumeRepository = resumeRepository;

    [HttpGet("{id:int}")]
    public ActionResult<Item> GetSavedCv(int id)
    {
        var item = _resumeRepository.GetById(id);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }
}
