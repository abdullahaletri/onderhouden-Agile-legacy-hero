using ExcellentTaste.Core.Data;
using ExcellentTaste.Core.Models;

namespace ExcellentTaste.Core.Repositories;

public class OmzetRepository : RepositoryBase
{
    private readonly DatabaseConnection _database;

    public OmzetRepository(DatabaseConnection database)
    {
        _database = database;
    }

    public List<WeekOmzetRegel> GetWeekOmzet(int? jaar = null, int? week = null)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = """
SELECT FLOOR(YEARWEEK(r.datum, 3) / 100) AS jaar,
       MOD(YEARWEEK(r.datum, 3), 100) AS week,
       COALESCE(g.gerechtnaam, 'Onbekend') AS gerecht,
       SUM(b.aantal * b.prijs) AS omzet
FROM bestelling b
JOIN reservering r ON b.reservering_id = r.reservering_id
LEFT JOIN menuitem m ON b.menuitemcode = m.menuitemcode
LEFT JOIN subgerecht s ON m.subgerechtcode = s.subgerechtcode
LEFT JOIN gerecht g ON s.gerechtcode = g.gerechtcode
WHERE r.totaal_bedrag IS NOT NULL
  AND (@jaar IS NULL OR FLOOR(YEARWEEK(r.datum, 3) / 100) = @jaar)
  AND (@week IS NULL OR MOD(YEARWEEK(r.datum, 3), 100) = @week)
GROUP BY FLOOR(YEARWEEK(r.datum, 3) / 100),
         MOD(YEARWEEK(r.datum, 3), 100),
         COALESCE(g.gerechtnaam, 'Onbekend')
ORDER BY jaar DESC, week DESC, gerecht
""";

        Add(command, "@jaar", jaar);
        Add(command, "@week", week);

        using var reader = command.ExecuteReader();

        var regels = new List<WeekOmzetRegel>();

        while (reader.Read())
        {
            regels.Add(new WeekOmzetRegel
            {
                Jaar = Int(reader, "jaar"),
                Week = Int(reader, "week"),
                Gerecht = Text(reader, "gerecht"),
                Omzet = Decimal(reader, "omzet")
            });
        }

        return regels;
    }
}
