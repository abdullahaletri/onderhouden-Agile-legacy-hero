using System.Net.Http.Json;
using ExcellentTaste.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExcellentTaste.Web.Controllers;

public class BonController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BonController> _logger;

    public BonController(IHttpClientFactory httpClientFactory, ILogger<BonController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int reservering)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ExcellentTasteApi");
            var bon = await client.GetFromJsonAsync<List<BonRegel>>($"api/bestellingen/{reservering}/bon") ?? [];
            return View(bon);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bon kon niet via de API worden opgehaald.");
            ViewBag.ApiFout = "De API is niet bereikbaar. Neem contact op met de administrator.";
            return View(new List<BonRegel>());
        }
    }
}
