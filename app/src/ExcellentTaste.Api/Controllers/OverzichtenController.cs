using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Api.Controllers;

[ApiController]
[Route("api/overzichten")]
public class OverzichtenController : ControllerBase
{
    private readonly OverzichtRepository _overzichten;

    public OverzichtenController(OverzichtRepository overzichten)
    {
        _overzichten = overzichten;
    }

    [HttpGet("kok")]
    public ActionResult<List<OverzichtRegel>> GetKok()
    {
        var regels = _overzichten.GetVoorOber();

        // @TODO: Test of de prijs in Nederlandse valuta word getoond
        Console.WriteLine(regels.ToArray()[0].MenuItemNaam);

        return Ok(regels);
    }

    [HttpGet("ober")]
    public ActionResult<List<OverzichtRegel>> GetOber()
    {
        var regels = _overzichten.GetVoorKok();

        // @TODO: Test of de prijs in Nederlandse valuta word getoond
        Console.WriteLine(regels.ToArray()[0].MenuItemNaam);

        return Ok(regels);
    }
}
