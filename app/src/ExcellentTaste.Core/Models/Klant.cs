namespace ExcellentTaste.Core.Models;

public class Klant
{
    public int KlantId { get; set; }
    public string KlantNaam { get; set; } = "";
    public string Telefoon { get; set; } = "";
    public string Email { get; set; } = "";
    public int Status { get; set; } = 1;
}
