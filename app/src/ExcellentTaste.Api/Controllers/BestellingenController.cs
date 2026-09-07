using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Api.Controllers;

[ApiController]
[Route("api/bestellingen")]
public class BestellingenController : ControllerBase
{
    private readonly BestellingService _service;

    public BestellingenController(BestellingService service)
    {
        _service = service;
    }

    [HttpGet("{reserveringId:int}")]
    public ActionResult<List<Bestelling>> GetByReservering(int reserveringId)
    {
        return Ok(_service.GetByReservering(reserveringId));
    }

    [HttpPost("{reserveringId:int}/items/{menuItemCode}")]
    public IActionResult AddItem(int reserveringId, string menuItemCode)
    {
        return _service.AddItem(reserveringId, menuItemCode)
            ? Ok()
            : NotFound();
    }

    [HttpPost("{reserveringId:int}/items/{menuItemCode}/plus")]
    public IActionResult PlusItem(int reserveringId, string menuItemCode)
    {
        _service.PlusItem(reserveringId, menuItemCode);
        return Ok();
    }

    [HttpPost("{reserveringId:int}/items/{menuItemCode}/min")]
    public IActionResult MinItem(int reserveringId, string menuItemCode)
    {
        _service.MinItem(reserveringId, menuItemCode);
        return Ok();
    }

    [HttpDelete("{reserveringId:int}/items/{menuItemCode}")]
    public IActionResult DeleteItem(int reserveringId, string menuItemCode)
    {
        _service.DeleteItem(reserveringId, menuItemCode);
        return NoContent();
    }

    [HttpGet("{reserveringId:int}/bon")]
    public ActionResult<List<BonRegel>> GetBon(int reserveringId)
    {
        return Ok(_service.GetBon(reserveringId));
    }
}
