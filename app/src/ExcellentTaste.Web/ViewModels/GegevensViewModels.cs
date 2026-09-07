using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExcellentTaste.Web.ViewModels;

public class GegevensIndexViewModel
{
    public string Soort { get; set; } = "";
    public string? ApiFout { get; set; }
    public List<GegevensRowViewModel> Rows { get; set; } = [];
}

public class GegevensRowViewModel
{
    public string Key { get; set; } = "";
    public List<string> Values { get; set; } = [];
}

public class GegevensFormViewModel
{
    public string Soort { get; set; } = "";
    public string? Key { get; set; }
    public string? Code { get; set; }
    public string? Omschrijving { get; set; }
    public string? Prijs { get; set; }
    public string? ValtOnder { get; set; }
    public string? Naam { get; set; }
    public string? Telefoon { get; set; }
    public string? Email { get; set; }
    public List<SelectListItem> ValtOnderOpties { get; set; } = [];
}

public class GegevensOkViewModel
{
    public string Soort { get; set; } = "";
    public string Tekst { get; set; } = "";
}
