using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Repositories;

namespace ExcellentTaste.Core.Services;

public class OmzetService
{
    private readonly OmzetRepository _omzet;

    public OmzetService(OmzetRepository omzet)
    {
        _omzet = omzet;
    }

    public List<WeekOmzetRegel> GetWeekOmzet(int? jaar = null, int? week = null) => _omzet.GetWeekOmzet(jaar, week);
}
