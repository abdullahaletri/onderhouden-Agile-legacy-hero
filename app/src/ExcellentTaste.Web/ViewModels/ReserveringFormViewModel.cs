using System.ComponentModel.DataAnnotations;
using ExcellentTaste.Core.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExcellentTaste.Web.ViewModels;

public class ReserveringFormViewModel
{
    public int ReserveringId { get; set; }

    [Display(Name = "Klant")]
    public int KlantId { get; set; } = -1;

    [Display(Name = "Klantnaam")]
    public string? KlantNaam { get; set; }

    [Display(Name = "E-mailadres klant")]
    public string? Email { get; set; }

    [Display(Name = "Telefoonnummer klant")]
    public string? Telefoon { get; set; }

    [Display(Name = "Reserveringsdatum (dd-mm-jjjj)")]
    public string Datum { get; set; } = "";

    [Display(Name = "Reserveringstijd (uu:mm)")]
    public string Tijd { get; set; } = "";

    [Range(1, 10, ErrorMessage = "Kies een tafelnummer van 1 tot en met 10.")]
    [Display(Name = "Tafelnummer")]
    public int Tafel { get; set; }

    [Range(1, 100, ErrorMessage = "Vul minimaal 1 persoon in.")]
    [Display(Name = "Aantal personen")]
    public int Aantal { get; set; }

    public List<SelectListItem> Klanten { get; set; } = [];

    public Reservering ToReservering(int klantId)
    {
        DateOnly.TryParseExact(Datum, "dd-MM-yyyy", out var datum);
        TimeOnly.TryParseExact(Tijd, "HH:mm", out var tijd);

        return new Reservering
        {
            ReserveringId = ReserveringId,
            KlantId = klantId,
            Datum = datum,
            Tijd = tijd,
            Tafel = Tafel,
            Aantal = Aantal,
            Status = 1
        };
    }
}
