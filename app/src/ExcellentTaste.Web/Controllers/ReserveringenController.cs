using System.Globalization;
using System.Net.Http.Json;
using ExcellentTaste.Core.Models;
using ExcellentTaste.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExcellentTaste.Web.Controllers;

public class ReserveringenController : Controller
{
    private const int NieuweKlantId = -1;
    private static readonly CultureInfo DutchCulture = CultureInfo.GetCultureInfo("nl-NL");
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReserveringenController> _logger;

    public ReserveringenController(IHttpClientFactory httpClientFactory, ILogger<ReserveringenController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var reserveringen = await GetReserveringen();
        return View(reserveringen);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReserveringFormViewModel model)
    {
        var klantId = await GetOrCreateKlantId(model);
        if (klantId < 1)
        {
            return View(await BuildFormModel(model));
        }

        var result = await PostApiResult("api/reserveringen", model.ToReservering(klantId));
        if (!result.Gelukt)
        {
            ModelState.AddModelError("", result.Bericht);
            return View(await BuildFormModel(model));
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var reservering = await GetReservering(id);
        if (reservering is null)
        {
            return NotFound();
        }

        return View(await BuildFormModel(FromReservering(reservering)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReserveringFormViewModel model)
    {
        model.ReserveringId = id;
        var klantId = await GetOrCreateKlantId(model);
        if (klantId < 1)
        {
            return View(await BuildFormModel(model));
        }

        var result = await PutApiResult($"api/reserveringen/{id}", model.ToReservering(klantId));
        if (!result.Gelukt)
        {
            ModelState.AddModelError("", result.Bericht);
            return View(await BuildFormModel(model));
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var reservering = await GetReservering(id);
        if (reservering is null)
        {
            return NotFound();
        }

        return View(reservering);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var client = CreateApiClient();
        await client.DeleteAsync($"api/reserveringen/{id}");
        return RedirectToAction(nameof(Index));
    }

    private HttpClient CreateApiClient()
    {
        return _httpClientFactory.CreateClient("ExcellentTasteApi");
    }

    private async Task<List<Reservering>> GetReserveringen()
    {
        try
        {
            var client = CreateApiClient();
            return await client.GetFromJsonAsync<List<Reservering>>("api/reserveringen") ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "De reserveringen konden niet via de API worden opgehaald.");
            ViewBag.ApiFout = "De API is niet bereikbaar. Neem contact op met de administrator.";
            return [];
        }
    }

    private async Task<Reservering?> GetReservering(int id)
    {
        try
        {
            var client = CreateApiClient();
            return await client.GetFromJsonAsync<Reservering>($"api/reserveringen/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reservering {ReserveringId} kon niet via de API worden opgehaald.", id);
            return null;
        }
    }

    private async Task<ReserveringFormViewModel> BuildFormModel(ReserveringFormViewModel? model = null)
    {
        model ??= new ReserveringFormViewModel();
        model.Klanten = await GetKlantenSelectList(model.KlantId);
        return model;
    }

    private async Task<List<SelectListItem>> GetKlantenSelectList(int geselecteerdeKlantId)
    {
        var items = new List<SelectListItem>
        {
            new("--Nieuwe klant--", NieuweKlantId.ToString(CultureInfo.InvariantCulture), geselecteerdeKlantId == NieuweKlantId)
        };

        try
        {
            var client = CreateApiClient();
            var klanten = await client.GetFromJsonAsync<List<Klant>>("api/klanten") ?? [];
            items.AddRange(klanten
                .Where(klant => klant.Status == 1)
                .Select(klant => new SelectListItem(
                    klant.KlantNaam,
                    klant.KlantId.ToString(CultureInfo.InvariantCulture),
                    klant.KlantId == geselecteerdeKlantId)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Klanten konden niet via de API worden opgehaald.");
            ModelState.AddModelError("", "Klanten konden niet via de API worden opgehaald.");
        }

        return items;
    }

    private async Task<int> GetOrCreateKlantId(ReserveringFormViewModel model)
    {
        if (model.KlantId != NieuweKlantId)
        {
            return model.KlantId;
        }

        if (string.IsNullOrWhiteSpace(model.KlantNaam))
        {
            ModelState.AddModelError(nameof(model.KlantNaam), "Vul een klantnaam in voor een nieuwe klant.");
            return 0;
        }

        var client = CreateApiClient();
        var response = await client.PostAsJsonAsync("api/klanten", new Klant
        {
            KlantNaam = model.KlantNaam ?? "",
            Email = model.Email ?? "",
            Telefoon = model.Telefoon ?? "",
            Status = 1
        });

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "De nieuwe klant kon niet worden opgeslagen.");
            return 0;
        }

        var result = await response.Content.ReadFromJsonAsync<KlantCreateResult>();
        return result?.Id ?? 0;
    }

    private bool ValidateDatumEnTijd(ReserveringFormViewModel model)
    {
        if (!DateOnly.TryParseExact(model.Datum, "dd-MM-yyyy", DutchCulture, DateTimeStyles.None, out _))
        {
            ModelState.AddModelError(nameof(model.Datum), "Gebruik de notatie dd-mm-jjjj.");
        }

        if (!TimeOnly.TryParseExact(model.Tijd, "HH:mm", DutchCulture, DateTimeStyles.None, out _))
        {
            ModelState.AddModelError(nameof(model.Tijd), "Gebruik de notatie uu:mm.");
        }

        return ModelState.IsValid;
    }

    private async Task<ApiResult> PostApiResult(string url, Reservering reservering)
    {
        var client = CreateApiClient();
        var response = await client.PostAsJsonAsync(url, reservering);
        return await ReadApiResult(response);
    }

    private async Task<ApiResult> PutApiResult(string url, Reservering reservering)
    {
        var client = CreateApiClient();
        var response = await client.PutAsJsonAsync(url, reservering);
        return await ReadApiResult(response);
    }

    private static async Task<ApiResult> ReadApiResult(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            return new ApiResult(false, "De reservering kon niet worden opgeslagen.");
        }

        return await response.Content.ReadFromJsonAsync<ApiResult>() ?? new ApiResult(true, "");
    }

    private static ReserveringFormViewModel FromReservering(Reservering reservering)
    {
        return new ReserveringFormViewModel
        {
            ReserveringId = reservering.ReserveringId,
            KlantId = reservering.KlantId,
            KlantNaam = reservering.KlantNaam,
            Telefoon = reservering.Telefoon,
            Datum = reservering.Datum.ToString("dd-MM-yyyy", DutchCulture),
            Tijd = reservering.Tijd.ToString("HH:mm", DutchCulture),
            Tafel = reservering.Tafel,
            Aantal = reservering.Aantal
        };
    }

    private sealed record ApiResult(bool Gelukt, string Bericht, int Id = 0);

    private sealed record KlantCreateResult(int Id);
}
