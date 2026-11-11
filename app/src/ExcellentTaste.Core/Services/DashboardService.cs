using System.Globalization;
using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Repositories;

namespace ExcellentTaste.Core.Services;

public class DashboardService
{
    private readonly DashboardRepository _dashboard;

    public DashboardService(DashboardRepository dashboard)
    {
        _dashboard = dashboard;
    }

    public DashboardData Get()
    {
        return new DashboardData
        {
            OmzetVandaag = _dashboard.GetOmzetVandaag(),
            OmzetDezeWeek = _dashboard.GetOmzetDezeWeek(),
            AantalBestellingenVandaag = _dashboard.GetAantalBestellingenVandaag(),
            OmzetPerBetaalwijze = _dashboard.GetOmzetPerBetaalwijze()
                .Select(x => new BetaalwijzeOmzet
                {
                    Betaalwijze = Enum.IsDefined(typeof(Betaalwijze), x.Betaalwijze)
                        ? ((Betaalwijze)x.Betaalwijze).ToString()
                        : "Onbekend",
                    Omzet = x.Omzet
                }).ToList(),
            OmzetPerHoofdgroep = _dashboard.GetOmzetPerHoofdgroepDezeWeek()
                .Select(x => new HoofdgroepOmzet { Hoofdgroep = x.Hoofdgroep, Omzet = x.Omzet })
                .ToList(),
            OmzetPerWeek = BuildLaatste8Weken()
        };
    }

    private List<WeekOmzet> BuildLaatste8Weken()
    {
        var perWeek = _dashboard.GetOmzetPerWeek().ToDictionary(x => x.WeekStart, x => x.Omzet);

        var today = DateTime.Today;
        int dagenSindsMaandag = ((int)today.DayOfWeek + 6) % 7;
        var dezeMaandag = DateOnly.FromDateTime(today.AddDays(-dagenSindsMaandag));

        var result = new List<WeekOmzet>();
        for (int i = 7; i >= 0; i--)
        {
            var weekStart = dezeMaandag.AddDays(-7 * i);
            result.Add(new WeekOmzet
            {
                WeekStart = weekStart,
                Weeknummer = ISOWeek.GetWeekOfYear(weekStart.ToDateTime(TimeOnly.MinValue)),
                Omzet = perWeek.TryGetValue(weekStart, out var omzet) ? omzet : 0m
            });
        }
        return result;
    }
}