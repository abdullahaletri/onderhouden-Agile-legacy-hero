using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Api.Controllers;

[ApiController]
[Route("api/menuitems")]
public class MenuItemsController : ControllerBase
{
    private readonly MenuRepository _menu;

    public MenuItemsController(MenuRepository menu)
    {
        _menu = menu;
    }

    [HttpGet]
    public ActionResult<List<MenuItem>> GetAll()
    {
        return Ok(_menu.GetMenuItems());
    }

    [HttpGet("gerechten")]
    public ActionResult<List<Gerecht>> GetGerechten()
    {
        return Ok(_menu.GetGerechten());
    }

    [HttpGet("subgerechten")]
    public ActionResult<List<Subgerecht>> GetSubgerechten()
    {
        return Ok(_menu.GetSubgerechten());
    }

    [HttpGet("{menuItemCode}")]
    public ActionResult<MenuItem> GetByCode(string menuItemCode)
    {
        var item = _menu.GetByCode(menuItemCode);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public IActionResult Add(MenuItem item)
    {
        _menu.AddMenuItem(item);
        return CreatedAtAction(nameof(GetByCode), new { menuItemCode = item.MenuItemCode }, item);
    }

    [HttpPut("{menuItemCode}")]
    public IActionResult Update(string menuItemCode, MenuItem item)
    {
        _menu.UpdateMenuItem(menuItemCode, item);
        return Ok();
    }

    [HttpDelete("{menuItemCode}")]
    public IActionResult Delete(string menuItemCode)
    {
        _menu.DeleteMenuItem(menuItemCode);
        return NoContent();
    }

    [HttpGet("gerechten/{gerechtCode}")]
    public ActionResult<Gerecht> GetGerechtByCode(string gerechtCode)
    {
        var gerecht = _menu.GetGerechtByCode(gerechtCode);
        return gerecht is null ? NotFound() : Ok(gerecht);
    }

    [HttpPost("gerechten")]
    public IActionResult AddGerecht(Gerecht gerecht)
    {
        _menu.AddGerecht(gerecht);
        return CreatedAtAction(nameof(GetGerechtByCode), new { gerechtCode = gerecht.GerechtCode }, gerecht);
    }

    [HttpPut("gerechten/{gerechtCode}")]
    public IActionResult UpdateGerecht(string gerechtCode, Gerecht gerecht)
    {
        _menu.UpdateGerecht(gerechtCode, gerecht);
        return Ok();
    }

    [HttpDelete("gerechten/{gerechtCode}")]
    public IActionResult DeleteGerecht(string gerechtCode)
    {
        _menu.DeleteGerecht(gerechtCode);
        return NoContent();
    }

    [HttpGet("subgerechten/{subgerechtCode}")]
    public ActionResult<Subgerecht> GetSubgerechtByCode(string subgerechtCode)
    {
        var subgerecht = _menu.GetSubgerechtByCode(subgerechtCode);
        return subgerecht is null ? NotFound() : Ok(subgerecht);
    }

    [HttpPost("subgerechten")]
    public IActionResult AddSubgerecht(Subgerecht subgerecht)
    {
        _menu.AddSubgerecht(subgerecht);
        return CreatedAtAction(nameof(GetSubgerechtByCode), new { subgerechtCode = subgerecht.SubgerechtCode }, subgerecht);
    }

    [HttpPut("subgerechten/{subgerechtCode}")]
    public IActionResult UpdateSubgerecht(string subgerechtCode, Subgerecht subgerecht)
    {
        _menu.UpdateSubgerecht(subgerechtCode, subgerecht);
        return Ok();
    }

    [HttpDelete("subgerechten/{subgerechtCode}")]
    public IActionResult DeleteSubgerecht(string subgerechtCode)
    {
        _menu.DeleteSubgerecht(subgerechtCode);
        return NoContent();
    }
}
