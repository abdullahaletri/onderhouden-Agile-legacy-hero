namespace ExcellentTaste.Core.Models;

public class WeekOmzetRegel
{
    public int Jaar { get; set; }
    public int Week { get; set; }
    public string Gerecht { get; set; } = "";
    public decimal Omzet { get; set; }
}
