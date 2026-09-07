using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Api.Controllers;

[ApiController]
[Route("api/klanten")]
public class KlantenController : ControllerBase
{
    private readonly KlantRepository _klanten;

    public KlantenController(KlantRepository klanten)
    {
        _klanten = klanten;
    }

    [HttpGet]
    public ActionResult<List<Klant>> GetAll()
    {
        return Ok(_klanten.GetAll());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Klant> GetById(int id)
    {
        var klant = _klanten.GetById(id);
        return klant is null ? NotFound() : Ok(klant);
    }

    [HttpPost]
    public ActionResult<object> Add(Klant klant)
    {
        var id = _klanten.Add(klant);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, Klant klant)
    {
        _klanten.Update(id, klant);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        _klanten.Delete(id);
        return NoContent();
    }
}
