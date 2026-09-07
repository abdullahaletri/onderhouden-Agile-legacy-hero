using System.Net;
using System.Text;
using System.Text.Json;
using ExcellentTaste.Core.Models;

namespace ExcellentTaste.WinForms.Api;

internal sealed class ExcellentTasteApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public ExcellentTasteApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public void CheckStatus()
    {
        Send(HttpMethod.Get, "api/status");
    }

    public List<Reservering> GetReserveringen() => Get<List<Reservering>>("api/reserveringen");

    public Reservering? GetReservering(int id) => GetOrNull<Reservering>($"api/reserveringen/{id}");

    public ApiResult AddReservering(Reservering reservering) => Post<ApiResult>("api/reserveringen", reservering);

    public ApiResult UpdateReservering(int id, Reservering reservering) => Put<ApiResult>($"api/reserveringen/{id}", reservering);

    public void DeleteReservering(int id)
    {
        Send(HttpMethod.Delete, $"api/reserveringen/{id}");
    }

    public List<Klant> GetKlanten() => Get<List<Klant>>("api/klanten");

    public Klant? GetKlant(int id) => GetOrNull<Klant>($"api/klanten/{id}");

    public int AddKlant(Klant klant) => Post<CreatedId>("api/klanten", klant).Id;

    public void UpdateKlant(int id, Klant klant)
    {
        Put($"api/klanten/{id}", klant);
    }

    public void DeleteKlant(int id)
    {
        Send(HttpMethod.Delete, $"api/klanten/{id}");
    }

    public List<MenuItem> GetMenuItems(string? soort = null)
    {
        if (string.IsNullOrWhiteSpace(soort))
        {
            return Get<List<MenuItem>>("api/menuitems");
        }

        return Get<List<MenuItem>>($"api/gegevens/{Uri.EscapeDataString(soort)}");
    }

    public MenuItem? GetMenuItem(string code) => GetOrNull<MenuItem>($"api/menuitems/{Uri.EscapeDataString(code)}");

    public void AddMenuItem(MenuItem item)
    {
        Post("api/menuitems", item);
    }

    public void UpdateMenuItem(string code, MenuItem item)
    {
        Put($"api/menuitems/{Uri.EscapeDataString(code)}", item);
    }

    public void DeleteMenuItem(string code)
    {
        Send(HttpMethod.Delete, $"api/menuitems/{Uri.EscapeDataString(code)}");
    }

    public List<Gerecht> GetGerechten() => Get<List<Gerecht>>("api/menuitems/gerechten");

    public Gerecht? GetGerecht(string code) => GetOrNull<Gerecht>($"api/menuitems/gerechten/{Uri.EscapeDataString(code)}");

    public void AddGerecht(Gerecht gerecht)
    {
        Post("api/menuitems/gerechten", gerecht);
    }

    public void UpdateGerecht(string code, Gerecht gerecht)
    {
        Put($"api/menuitems/gerechten/{Uri.EscapeDataString(code)}", gerecht);
    }

    public void DeleteGerecht(string code)
    {
        Send(HttpMethod.Delete, $"api/menuitems/gerechten/{Uri.EscapeDataString(code)}");
    }

    public List<Subgerecht> GetSubgerechten() => Get<List<Subgerecht>>("api/menuitems/subgerechten");

    public Subgerecht? GetSubgerecht(string code) => GetOrNull<Subgerecht>($"api/menuitems/subgerechten/{Uri.EscapeDataString(code)}");

    public void AddSubgerecht(Subgerecht subgerecht)
    {
        Post("api/menuitems/subgerechten", subgerecht);
    }

    public void UpdateSubgerecht(string code, Subgerecht subgerecht)
    {
        Put($"api/menuitems/subgerechten/{Uri.EscapeDataString(code)}", subgerecht);
    }

    public void DeleteSubgerecht(string code)
    {
        Send(HttpMethod.Delete, $"api/menuitems/subgerechten/{Uri.EscapeDataString(code)}");
    }

    public List<OverzichtRegel> GetOverzicht(string soort) => Get<List<OverzichtRegel>>($"api/overzichten/{Uri.EscapeDataString(soort)}");

    public List<Bestelling> GetBestellingen(int reserveringId) => Get<List<Bestelling>>($"api/bestellingen/{reserveringId}");

    public void AddBestellingItem(int reserveringId, string menuItemCode)
    {
        Post($"api/bestellingen/{reserveringId}/items/{Uri.EscapeDataString(menuItemCode)}");
    }

    public void PlusBestellingItem(int reserveringId, string menuItemCode)
    {
        Post($"api/bestellingen/{reserveringId}/items/{Uri.EscapeDataString(menuItemCode)}/plus");
    }

    public void MinBestellingItem(int reserveringId, string menuItemCode)
    {
        Post($"api/bestellingen/{reserveringId}/items/{Uri.EscapeDataString(menuItemCode)}/min");
    }

    public void DeleteBestellingItem(int reserveringId, string menuItemCode)
    {
        Send(HttpMethod.Delete, $"api/bestellingen/{reserveringId}/items/{Uri.EscapeDataString(menuItemCode)}");
    }

    public List<BonRegel> GetBon(int reserveringId) => Get<List<BonRegel>>($"api/bestellingen/{reserveringId}/bon");

    private T Get<T>(string path) => Read<T>(Send(HttpMethod.Get, path));

    private T? GetOrNull<T>(string path)
    {
        using HttpResponseMessage response = Send(HttpMethod.Get, path, allowNotFound: true);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        return Read<T>(response);
    }

    private void Post(string path)
    {
        Send(HttpMethod.Post, path);
    }

    private T Post<T>(string path, object body) => Read<T>(Send(HttpMethod.Post, path, body));

    private void Post(string path, object body)
    {
        Send(HttpMethod.Post, path, body);
    }

    private T Put<T>(string path, object body) => Read<T>(Send(HttpMethod.Put, path, body));

    private void Put(string path, object body)
    {
        Send(HttpMethod.Put, path, body);
    }

    private HttpResponseMessage Send(HttpMethod method, string path, object? body = null, bool allowNotFound = false)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(method, path);
            if (body != null)
            {
                string json = JsonSerializer.Serialize(body, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage response = _httpClient.Send(request);
            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
            {
                return response;
            }

            response.EnsureSuccessStatusCode();
            return response;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            throw new ApiUnavailableException("De API is niet bereikbaar. Neem contact op met de administrator.", ex);
        }
    }

    private static T Read<T>(HttpResponseMessage response)
    {
        using (response)
        {
            Stream stream = response.Content.ReadAsStream();
            T? value = JsonSerializer.Deserialize<T>(stream, JsonOptions);
            if (value == null)
            {
                throw new InvalidOperationException("De API gaf geen geldig antwoord terug.");
            }

            return value;
        }
    }
}

internal sealed class ApiResult
{
    public bool Gelukt { get; set; }

    public string Bericht { get; set; } = "";

    public int Id { get; set; }
}

internal sealed class CreatedId
{
    public int Id { get; set; }
}
