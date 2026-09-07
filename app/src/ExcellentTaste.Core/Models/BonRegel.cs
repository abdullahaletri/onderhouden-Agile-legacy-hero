namespace ExcellentTaste.Core.Models;

public class BonRegel
{
    public string MenuItemNaam { get; set; } = "";
    public int Aantal { get; set; }
    public decimal Prijs { get; set; }
    public decimal Totaal => Aantal * Prijs;
}
