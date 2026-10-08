using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Repositories;

namespace ExcellentTaste.Core.Services;

public class ReserveringService
{
    private readonly ReserveringRepository _reserveringen;

    public ReserveringService(ReserveringRepository reserveringen)
    {
        _reserveringen = reserveringen;
    }

    public List<Reservering> GetAll() => _reserveringen.GetAll();

    public Reservering? GetById(int id) => _reserveringen.GetById(id);

    public (bool Gelukt, string Bericht, int Id) Add(Reservering reservering)
    {
        if (_reserveringen.IsTafelBezet(reservering.Tafel, reservering.Datum, reservering.Tijd))
        {
            return (false, "Deze tafel is op dit tijdstip al gereserveerd.", 0);
        }

        var id = _reserveringen.Add(reservering);
        return (true, "Reservering opgeslagen.", id);
    }

    public (bool Gelukt, string Bericht) Update(int id, Reservering reservering)
    {
        if (_reserveringen.IsTafelBezet(reservering.Tafel, reservering.Datum, reservering.Tijd, id))
        {
            return (false, "Deze tafel is op dit tijdstip al gereserveerd.");
        }

        _reserveringen.Update(id, reservering);
        return (true, "Reservering opgeslagen.");
    }

    public void Delete(int id) => _reserveringen.Delete(id);
}
