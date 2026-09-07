using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Api.Controllers;

[ApiController]
[Route("api/reserveringen")]
public class ReserveringenController : ControllerBase
{
    private readonly ReserveringService _service;

    public ReserveringenController(ReserveringService service)
    {
        _service = service;
    }

    [HttpGet]
    public ActionResult<List<Reservering>> GetAll()
    {
        return Ok(_service.GetAll());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Reservering> GetById(int id)
    {
        var reservering = _service.GetById(id);
        if (reservering is null)
        {
            return NotFound();
        }

        return Ok(reservering);
    }

    [HttpPost]
    public ActionResult<object> Add(Reservering reservering)
    {
        var result = _service.Add(reservering);
        if (!result.Gelukt)
        {
            // Legacy issue: dit zou eigenlijk 409 Conflict moeten zijn.
            return Ok(new { result.Gelukt, result.Bericht });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { result.Gelukt, result.Bericht, result.Id });
    }

    [HttpPut("{id:int}")]
    public ActionResult<object> Update(int id, Reservering reservering)
    {
        var result = _service.Update(id, reservering);
        if (!result.Gelukt)
        {
            // Legacy issue: dit zou eigenlijk 409 Conflict moeten zijn.
            return Ok(new { result.Gelukt, result.Bericht });
        }

        return Ok(new { result.Gelukt, result.Bericht });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _service.Delete(id);
        return NoContent();
    }
}
