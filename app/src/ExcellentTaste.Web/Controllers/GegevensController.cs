using System.Globalization;
using System.Net.Http.Json;
using ExcellentTaste.Core.Models;
using ExcellentTaste.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExcellentTaste.Web.Controllers;

public class GegevensController : Controller
{
    private static readonly CultureInfo DutchCulture = CultureInfo.GetCultureInfo("nl-NL");
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GegevensController> _logger;

    public GegevensController(IHttpClientFactory httpClientFactory, ILogger<GegevensController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string soort)
    {
        if (!IsKnownSoort(soort))
        {
            return Content("Onbekend gegeven.");
        }

        return View(await BuildIndexModel(soort));
    }

    public async Task<IActionResult> Create(string soort)
    {
        if (!IsKnownSoort(soort))
        {
            return Content("Onbekend gegeven.");
        }

        return View(await BuildFormModel(new GegevensFormViewModel { Soort = soort }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GegevensFormViewModel model)
    {
        if (!ValidateForm(model))
        {
            return View(await BuildFormModel(model));
        }

        var response = await SendCreate(model);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "De gegevens zijn NIET opgeslagen.");
            return View(await BuildFormModel(model));
        }

        return View("Ok", new GegevensOkViewModel { Soort = model.Soort, Tekst = "De gegevens zijn opgeslagen." });
    }

    public async Task<IActionResult> Edit(string soort, string key)
    {
        if (!IsKnownSoort(soort))
        {
            return Content("Onbekend gegeven.");
        }

        var model = await GetFormModel(soort, key);
        if (model is null)
        {
            return View("Ok", new GegevensOkViewModel { Soort = soort, Tekst = "Onbekende gegevens." });
        }

        return View(await BuildFormModel(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(GegevensFormViewModel model)
    {
        if (!ValidateForm(model))
        {
            return View(await BuildFormModel(model));
        }

        var response = await SendUpdate(model);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "De gegevens zijn NIET opgeslagen.");
            return View(await BuildFormModel(model));
        }

        return View("Ok", new GegevensOkViewModel { Soort = model.Soort, Tekst = "De gegevens zijn opgeslagen." });
    }

    public async Task<IActionResult> Delete(string soort, string key)
    {
        if (!IsKnownSoort(soort) || string.IsNullOrWhiteSpace(key))
        {
            return View("Ok", new GegevensOkViewModel { Soort = soort, Tekst = "Onbekende gegevens." });
        }

        var response = await SendDelete(soort, key);
        return response.IsSuccessStatusCode
            ? View("Ok", new GegevensOkViewModel { Soort = soort, Tekst = "De gegevens zijn verwijderd." })
            : View("Ok", new GegevensOkViewModel { Soort = soort, Tekst = "De gegevens zijn NIET verwijderd." });
    }

    private async Task<GegevensIndexViewModel> BuildIndexModel(string soort)
    {
        var model = new GegevensIndexViewModel { Soort = soort };
        try
        {
            var client = CreateApiClient();
            switch (soort)
            {
                case "drinken":
                case "eten":
                    var menuItems = await client.GetFromJsonAsync<List<MenuItem>>($"api/gegevens/{soort}") ?? [];
                    var subgerechten = await client.GetFromJsonAsync<List<Subgerecht>>("api/menuitems/subgerechten") ?? [];
                    var lookup = subgerechten.ToDictionary(item => item.SubgerechtCode, item => item.SubgerechtNaam);
                    model.Rows = menuItems.Select(item => new GegevensRowViewModel
                    {
                        Key = item.MenuItemCode,
                        Values =
                        [
                            item.MenuItemCode,
                            item.MenuItemNaam,
                            item.Prijs.ToString("0.##", DutchCulture),
                            lookup.GetValueOrDefault(item.SubgerechtCode, item.SubgerechtCode)
                        ]
                    }).ToList();
                    break;
                case "klanten":
                    var klanten = await client.GetFromJsonAsync<List<Klant>>("api/gegevens/klanten") ?? [];
                    model.Rows = klanten.Select(klant => new GegevensRowViewModel
                    {
                        Key = klant.KlantId.ToString(CultureInfo.InvariantCulture),
                        Values = [klant.KlantNaam, klant.Telefoon, klant.Email]
                    }).ToList();
                    break;
                case "gerechten":
                    var gerechten = await client.GetFromJsonAsync<List<Gerecht>>("api/gegevens/gerechten") ?? [];
                    model.Rows = gerechten.OrderBy(gerecht => gerecht.GerechtNaam).Select(gerecht => new GegevensRowViewModel
                    {
                        Key = gerecht.GerechtCode,
                        Values = [gerecht.GerechtCode, gerecht.GerechtNaam]
                    }).ToList();
                    break;
                case "subgerechten":
                    var subs = await client.GetFromJsonAsync<List<Subgerecht>>("api/gegevens/subgerechten") ?? [];
                    var gerechtenLookup = (await client.GetFromJsonAsync<List<Gerecht>>("api/menuitems/gerechten") ?? [])
                        .ToDictionary(item => item.GerechtCode, item => item.GerechtNaam);
                    model.Rows = subs.OrderBy(sub => sub.SubgerechtNaam).Select(sub => new GegevensRowViewModel
                    {
                        Key = sub.SubgerechtCode,
                        Values = [sub.SubgerechtCode, sub.SubgerechtNaam, gerechtenLookup.GetValueOrDefault(sub.GerechtCode, sub.GerechtCode)]
                    }).ToList();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gegevens konden niet via de API worden opgehaald.");
            model.ApiFout = "De API is niet bereikbaar. Neem contact op met de administrator.";
        }

        return model;
    }

    private async Task<GegevensFormViewModel?> GetFormModel(string soort, string key)
    {
        var client = CreateApiClient();
        return soort switch
        {
            "drinken" or "eten" => await GetMenuItemFormModel(client, soort, key),
            "klanten" => await GetKlantFormModel(client, soort, key),
            "gerechten" => await GetGerechtFormModel(client, soort, key),
            "subgerechten" => await GetSubgerechtFormModel(client, soort, key),
            _ => null
        };
    }

    private static async Task<GegevensFormViewModel?> GetMenuItemFormModel(HttpClient client, string soort, string key)
    {
        var item = await client.GetFromJsonAsync<MenuItem>($"api/menuitems/{Uri.EscapeDataString(key)}");
        return item is null ? null : new GegevensFormViewModel
        {
            Soort = soort,
            Key = key,
            Code = item.MenuItemCode,
            Omschrijving = item.MenuItemNaam,
            Prijs = item.Prijs.ToString("0.##", DutchCulture),
            ValtOnder = item.SubgerechtCode
        };
    }

    private static async Task<GegevensFormViewModel?> GetKlantFormModel(HttpClient client, string soort, string key)
    {
        var klant = await client.GetFromJsonAsync<Klant>($"api/klanten/{key}");
        return klant is null ? null : new GegevensFormViewModel
        {
            Soort = soort,
            Key = key,
            Naam = klant.KlantNaam,
            Telefoon = klant.Telefoon,
            Email = klant.Email
        };
    }

    private static async Task<GegevensFormViewModel?> GetGerechtFormModel(HttpClient client, string soort, string key)
    {
        var gerecht = await client.GetFromJsonAsync<Gerecht>($"api/menuitems/gerechten/{Uri.EscapeDataString(key)}");
        return gerecht is null ? null : new GegevensFormViewModel
        {
            Soort = soort,
            Key = key,
            Code = gerecht.GerechtCode,
            Omschrijving = gerecht.GerechtNaam
        };
    }

    private static async Task<GegevensFormViewModel?> GetSubgerechtFormModel(HttpClient client, string soort, string key)
    {
        var subgerecht = await client.GetFromJsonAsync<Subgerecht>($"api/menuitems/subgerechten/{Uri.EscapeDataString(key)}");
        return subgerecht is null ? null : new GegevensFormViewModel
        {
            Soort = soort,
            Key = key,
            Code = subgerecht.SubgerechtCode,
            Omschrijving = subgerecht.SubgerechtNaam,
            ValtOnder = subgerecht.GerechtCode
        };
    }

    private async Task<GegevensFormViewModel> BuildFormModel(GegevensFormViewModel model)
    {
        if (model.Soort is "drinken" or "eten")
        {
            model.ValtOnderOpties = await GetSubgerechtOpties(model.ValtOnder ?? "");
        }

        if (model.Soort == "subgerechten")
        {
            model.ValtOnderOpties = await GetGerechtOpties(model.ValtOnder ?? "");
        }

        return model;
    }

    private async Task<List<SelectListItem>> GetSubgerechtOpties(string selected)
    {
        var client = CreateApiClient();
        var subgerechten = await client.GetFromJsonAsync<List<Subgerecht>>("api/menuitems/subgerechten") ?? [];
        var gerechten = await client.GetFromJsonAsync<List<Gerecht>>("api/menuitems/gerechten") ?? [];
        var lookup = gerechten.ToDictionary(item => item.GerechtCode, item => item.GerechtNaam);
        return subgerechten.Select(subgerecht => new SelectListItem(
            $"{subgerecht.SubgerechtCode} - {subgerecht.SubgerechtNaam}->{lookup.GetValueOrDefault(subgerecht.GerechtCode, subgerecht.GerechtCode)}",
            subgerecht.SubgerechtCode,
            subgerecht.SubgerechtCode == selected)).ToList();
    }

    private async Task<List<SelectListItem>> GetGerechtOpties(string selected)
    {
        var client = CreateApiClient();
        var gerechten = await client.GetFromJsonAsync<List<Gerecht>>("api/menuitems/gerechten") ?? [];
        return gerechten.OrderBy(gerecht => gerecht.GerechtNaam).Select(gerecht => new SelectListItem(
            gerecht.GerechtNaam,
            gerecht.GerechtCode,
            gerecht.GerechtCode == selected)).ToList();
    }

    private bool ValidateForm(GegevensFormViewModel model)
    {
        if (model.Soort != "klanten" && string.IsNullOrWhiteSpace(model.Code) && string.IsNullOrWhiteSpace(model.Key))
        {
            ModelState.AddModelError(nameof(model.Code), "Vul een code in.");
        }

        if (model.Soort == "klanten" && string.IsNullOrWhiteSpace(model.Naam))
        {
            ModelState.AddModelError(nameof(model.Naam), "Vul een naam in.");
        }

        if (model.Soort != "klanten" && string.IsNullOrWhiteSpace(model.Omschrijving))
        {
            ModelState.AddModelError(nameof(model.Omschrijving), "Vul een omschrijving in.");
        }

        if (model.Soort is "drinken" or "eten")
        {
            if (!decimal.TryParse(model.Prijs, NumberStyles.Number, DutchCulture, out _) &&
                !decimal.TryParse(model.Prijs, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            {
                ModelState.AddModelError(nameof(model.Prijs), "Vul een prijs in.");
            }
        }

        return ModelState.IsValid;
    }

    private async Task<HttpResponseMessage> SendCreate(GegevensFormViewModel model)
    {
        var client = CreateApiClient();
        return model.Soort switch
        {
            "drinken" or "eten" => await client.PostAsJsonAsync("api/menuitems", ToMenuItem(model)),
            "klanten" => await client.PostAsJsonAsync("api/klanten", ToKlant(model)),
            "gerechten" => await client.PostAsJsonAsync("api/menuitems/gerechten", ToGerecht(model)),
            "subgerechten" => await client.PostAsJsonAsync("api/menuitems/subgerechten", ToSubgerecht(model)),
            _ => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
        };
    }

    private async Task<HttpResponseMessage> SendUpdate(GegevensFormViewModel model)
    {
        var client = CreateApiClient();
        return model.Soort switch
        {
            "drinken" or "eten" => await client.PutAsJsonAsync($"api/menuitems/{Uri.EscapeDataString(model.Key ?? "")}", ToMenuItem(model)),
            "klanten" => await client.PutAsJsonAsync($"api/klanten/{model.Key}", ToKlant(model)),
            "gerechten" => await client.PutAsJsonAsync($"api/menuitems/gerechten/{Uri.EscapeDataString(model.Key ?? "")}", ToGerecht(model)),
            "subgerechten" => await client.PutAsJsonAsync($"api/menuitems/subgerechten/{Uri.EscapeDataString(model.Key ?? "")}", ToSubgerecht(model)),
            _ => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
        };
    }

    private async Task<HttpResponseMessage> SendDelete(string soort, string key)
    {
        var client = CreateApiClient();
        return soort switch
        {
            "drinken" or "eten" => await client.DeleteAsync($"api/menuitems/{Uri.EscapeDataString(key)}"),
            "klanten" => await client.DeleteAsync($"api/klanten/{key}"),
            "gerechten" => await client.DeleteAsync($"api/menuitems/gerechten/{Uri.EscapeDataString(key)}"),
            "subgerechten" => await client.DeleteAsync($"api/menuitems/subgerechten/{Uri.EscapeDataString(key)}"),
            _ => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
        };
    }

    private HttpClient CreateApiClient() => _httpClientFactory.CreateClient("ExcellentTasteApi");

    private static bool IsKnownSoort(string soort)
    {
        return soort is "drinken" or "eten" or "klanten" or "gerechten" or "subgerechten";
    }

    private static MenuItem ToMenuItem(GegevensFormViewModel model)
    {
        return new MenuItem
        {
            MenuItemCode = string.IsNullOrWhiteSpace(model.Code) ? model.Key ?? "" : model.Code,
            MenuItemNaam = model.Omschrijving ?? "",
            Prijs = ParsePrijs(model.Prijs ?? "0"),
            SubgerechtCode = model.ValtOnder ?? ""
        };
    }

    private static Klant ToKlant(GegevensFormViewModel model)
    {
        return new Klant
        {
            KlantNaam = model.Naam ?? "",
            Telefoon = model.Telefoon ?? "",
            Email = model.Email ?? "",
            Status = 1
        };
    }

    private static Gerecht ToGerecht(GegevensFormViewModel model)
    {
        return new Gerecht
        {
            GerechtCode = string.IsNullOrWhiteSpace(model.Code) ? model.Key ?? "" : model.Code,
            GerechtNaam = model.Omschrijving ?? ""
        };
    }

    private static Subgerecht ToSubgerecht(GegevensFormViewModel model)
    {
        return new Subgerecht
        {
            SubgerechtCode = string.IsNullOrWhiteSpace(model.Code) ? model.Key ?? "" : model.Code,
            SubgerechtNaam = model.Omschrijving ?? "",
            GerechtCode = model.ValtOnder ?? ""
        };
    }

    private static decimal ParsePrijs(string prijs)
    {
        return decimal.TryParse(prijs, NumberStyles.Number, DutchCulture, out var dutchValue)
            ? dutchValue
            : decimal.Parse(prijs, NumberStyles.Number, CultureInfo.InvariantCulture);
    }
}
