using System.Net.Http.Json;
using ExcellentTaste.Core.Models;
using ExcellentTaste.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Web.Controllers;

public class OverzichtenController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OverzichtenController> _logger;

    public OverzichtenController(IHttpClientFactory httpClientFactory, ILogger<OverzichtenController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string overzicht = "kok")
    {
        var soort = overzicht == "ober" ? "ober" : "kok";
        var model = new OverzichtenViewModel
        {
            Overzicht = soort,
            Titel = soort == "kok" ? "Overzicht voor kok" : "Overzicht voor ober",
            ItemKolom = soort == "kok" ? "Gerecht" : "Drank"
        };

        try
        {
            var client = _httpClientFactory.CreateClient("ExcellentTasteApi");
            model.Regels = await client.GetFromJsonAsync<List<OverzichtRegel>>($"api/overzichten/{soort}") ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Overzicht {Overzicht} kon niet via de API worden opgehaald.", soort);
            model.ApiFout = "De API is niet bereikbaar. Neem contact op met de administrator.";
        }

        return View(model);
    }
}
