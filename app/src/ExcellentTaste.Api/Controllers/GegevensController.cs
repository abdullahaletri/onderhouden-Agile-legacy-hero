using ExcellentTaste.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Api.Controllers;

[ApiController]
[Route("api/gegevens")]
public class GegevensController : ControllerBase
{
    private readonly MenuRepository _menu;
    private readonly KlantRepository _klanten;

    public GegevensController(MenuRepository menu, KlantRepository klanten)
    {
        _menu = menu;
        _klanten = klanten;
    }

    [HttpGet("{soort}")]
    public IActionResult GetBySoort(string soort)
    {
        return soort.ToLowerInvariant() switch
        {
            "drinken" => Ok(_menu.GetMenuItems("drinken")),
            "eten" => Ok(_menu.GetMenuItems("eten")),
            "klanten" => Ok(_klanten.GetAll()),
            "gerechten" => Ok(_menu.GetGerechten()),
            "subgerechten" => Ok(_menu.GetSubgerechten()),
            _ => NotFound(new { bericht = "Onbekende gegevenssoort." })
        };
    }
}
