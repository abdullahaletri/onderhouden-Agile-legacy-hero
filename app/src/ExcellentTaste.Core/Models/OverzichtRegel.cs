namespace ExcellentTaste.Core.Models;

public class OverzichtRegel
{
    public int Tafel { get; set; }
    public int Aantal { get; set; }
    public string MenuItemNaam { get; set; } = "";
    public int ReserveringId { get; set; }
}
