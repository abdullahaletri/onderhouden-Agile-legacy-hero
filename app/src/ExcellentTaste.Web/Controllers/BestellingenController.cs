using System.Net.Http.Json;
using ExcellentTaste.Core.Models;
using ExcellentTaste.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Web.Controllers;

public class BestellingenController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BestellingenController> _logger;

    public BestellingenController(IHttpClientFactory httpClientFactory, ILogger<BestellingenController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int reservering, int? tafelnummer)
    {
        if (reservering <= 0)
        {
            return RedirectToAction("Index", "Reserveringen");
        }

        return View(await BuildViewModel(reservering, tafelnummer));
    }

    public async Task<IActionResult> Add(int reservering, string item)
    {
        if (reservering > 0 && !string.IsNullOrWhiteSpace(item))
        {
            var client = CreateApiClient();
            await client.PostAsync($"api/bestellingen/{reservering}/items/{Uri.EscapeDataString(item)}", null);
        }

        return RedirectToAction(nameof(Index), new { reservering });
    }

    public async Task<IActionResult> PlusItem(int reservering, string menuitemcode)
    {
        if (reservering > 0 && !string.IsNullOrWhiteSpace(menuitemcode))
        {
            var client = CreateApiClient();
            await client.PostAsync($"api/bestellingen/{reservering}/items/{Uri.EscapeDataString(menuitemcode)}/plus", null);
        }

        return RedirectToAction(nameof(Index), new { reservering });
    }

    public async Task<IActionResult> MinItem(int reservering, string menuitemcode)
    {
        if (reservering > 0 && !string.IsNullOrWhiteSpace(menuitemcode))
        {
            var client = CreateApiClient();
            await client.PostAsync($"api/bestellingen/{reservering}/items/{Uri.EscapeDataString(menuitemcode)}/min", null);
        }

        return RedirectToAction(nameof(Index), new { reservering });
    }

    public async Task<IActionResult> DeleteItem(int reservering, string menuitemcode)
    {
        if (reservering > 0 && !string.IsNullOrWhiteSpace(menuitemcode))
        {
            var client = CreateApiClient();
            await client.DeleteAsync($"api/bestellingen/{reservering}/items/{Uri.EscapeDataString(menuitemcode)}");
        }

        return RedirectToAction(nameof(Index), new { reservering });
    }

    private HttpClient CreateApiClient()
    {
        return _httpClientFactory.CreateClient("ExcellentTasteApi");
    }

    private async Task<BestellingenViewModel> BuildViewModel(int reserveringId, int? tafelnummer)
    {
        var model = new BestellingenViewModel
        {
            ReserveringId = reserveringId,
            Tafelnummer = tafelnummer ?? 0
        };

        try
        {
            var client = CreateApiClient();
            var reservering = await client.GetFromJsonAsync<Reservering>($"api/reserveringen/{reserveringId}");
            model.Tafelnummer = tafelnummer ?? reservering?.Tafel ?? 0;
            model.Bestellingen = await client.GetFromJsonAsync<List<Bestelling>>($"api/bestellingen/{reserveringId}") ?? [];
            model.Gerechten = await client.GetFromJsonAsync<List<Gerecht>>("api/menuitems/gerechten") ?? [];
            model.Subgerechten = await client.GetFromJsonAsync<List<Subgerecht>>("api/menuitems/subgerechten") ?? [];
            model.MenuItems = await client.GetFromJsonAsync<List<MenuItem>>("api/menuitems") ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bestellingen konden niet via de API worden opgehaald.");
            model.ApiFout = "De API is niet bereikbaar. Neem contact op met de administrator.";
        }

        return model;
    }
}
