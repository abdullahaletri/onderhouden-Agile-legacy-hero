using ExcellentTaste.Core.Data;
using ExcellentTaste.Core.Models;

namespace ExcellentTaste.Core.Repositories;

public class OverzichtRepository : RepositoryBase
{
    private readonly DatabaseConnection _database;

    public OverzichtRepository(DatabaseConnection database)
    {
        _database = database;
    }

    public List<OverzichtRegel> GetVoorKok()
    {
        return GetOverzicht("<> 'drk'");
    }

    public List<OverzichtRegel> GetVoorOber()
    {
        // Legacy issue: dit is eigenlijk het barmanoverzicht; het echte oberoverzicht ontbreekt nog.
        return GetOverzicht("= 'drk'");
    }

    private List<OverzichtRegel> GetOverzicht(string filter)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"""
SELECT b.tafel, b.aantal, m.menuitemnaam, r.reservering_id
FROM reservering r
LEFT JOIN bestelling b ON r.reservering_id = b.reservering_id
LEFT JOIN menuitem m ON b.menuitemcode = m.menuitemcode
LEFT JOIN subgerecht s ON m.subgerechtcode = s.subgerechtcode
WHERE r.datum = @vandaag AND s.gerechtcode {filter}
ORDER BY b.tijd, b.tafel
""";
        Add(command, "@vandaag", DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"));
        using var reader = command.ExecuteReader();

        var regels = new List<OverzichtRegel>();
        while (reader.Read())
        {
            regels.Add(new OverzichtRegel
            {
                Tafel = Int(reader, "tafel"),
                Aantal = Int(reader, "aantal"),
                MenuItemNaam = Text(reader, "menuitemnaam"),
                ReserveringId = Int(reader, "reservering_id")
            });
        }

        return regels;
    }
}
