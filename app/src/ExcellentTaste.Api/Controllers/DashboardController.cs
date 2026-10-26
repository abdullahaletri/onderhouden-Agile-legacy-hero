using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(DashboardService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<DashboardData> Get()
    {
        return Ok(_service.Get());
    }
}