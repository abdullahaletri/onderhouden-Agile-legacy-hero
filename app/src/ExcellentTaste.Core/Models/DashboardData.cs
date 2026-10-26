namespace ExcellentTaste.Core.Models;

public class DashboardData
{
    public decimal OmzetVandaag { get; set; }
    public decimal OmzetDezeWeek { get; set; }
    public int AantalBestellingenVandaag { get; set; }
    public List<BetaalwijzeOmzet> OmzetPerBetaalwijze { get; set; } = new();
    public List<HoofdgroepOmzet> OmzetPerHoofdgroep { get; set; } = new();
    public List<WeekOmzet> OmzetPerWeek { get; set; } = new();
}

public class BetaalwijzeOmzet
{
    public string Betaalwijze { get; set; } = "";
    public decimal Omzet { get; set; }
}

public class HoofdgroepOmzet
{
    public string Hoofdgroep { get; set; } = "";
    public decimal Omzet { get; set; }
}

public class WeekOmzet
{
    public DateOnly WeekStart { get; set; }
    public int Weeknummer { get; set; }
    public decimal Omzet { get; set; }
}