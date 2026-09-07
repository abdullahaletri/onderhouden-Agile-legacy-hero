using ExcellentTaste.Core.Models;

namespace ExcellentTaste.Web.ViewModels;

public class BestellingenViewModel
{
    public int ReserveringId { get; set; }
    public int Tafelnummer { get; set; }
    public List<Bestelling> Bestellingen { get; set; } = [];
    public List<Gerecht> Gerechten { get; set; } = [];
    public List<Subgerecht> Subgerechten { get; set; } = [];
    public List<MenuItem> MenuItems { get; set; } = [];
    public string? ApiFout { get; set; }
}
