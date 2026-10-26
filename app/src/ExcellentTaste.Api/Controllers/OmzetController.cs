using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Api.Controllers;

[ApiController]
[Route("api/omzet")]
public class OmzetController : ControllerBase
{
    private readonly OmzetService _service;

    public OmzetController(OmzetService service)
    {
        _service = service;
    }

    [HttpGet("week")]
    public ActionResult<List<WeekOmzetRegel>> GetWeekOmzet([FromQuery] int? jaar, [FromQuery] int? week)
    {
        return Ok(_service.GetWeekOmzet(jaar, week));
    }
}
