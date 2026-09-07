namespace ExcellentTaste.Core.Models;

public class Bestelling
{
    public int BestellingId { get; set; }
    public int ReserveringId { get; set; }
    public int Tafel { get; set; }
    public DateOnly Datum { get; set; }
    public TimeOnly Tijd { get; set; }
    public string MenuItemCode { get; set; } = "";
    public string MenuItemNaam { get; set; } = "";
    public int Aantal { get; set; }
    public decimal Prijs { get; set; }
}
