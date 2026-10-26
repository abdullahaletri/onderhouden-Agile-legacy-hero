using ExcellentTaste.Core.Data;

namespace ExcellentTaste.Core.Repositories;

public class DashboardRepository
{
    // Omzet = alleen reserveringen die afgerekend zijn.
    private const string Betaald = "r.betaalwijze IS NOT NULL AND r.totaal_bedrag IS NOT NULL";

    private readonly DatabaseConnection _database;

    public DashboardRepository(DatabaseConnection database)
    {
        _database = database;
    }

    public decimal GetOmzetVandaag() => ScalarDecimal($@"
SELECT COALESCE(SUM(r.totaal_bedrag), 0)
FROM reservering r
WHERE {Betaald} AND r.datum = CURDATE()");

    public decimal GetOmzetDezeWeek() => ScalarDecimal($@"
SELECT COALESCE(SUM(r.totaal_bedrag), 0)
FROM reservering r
WHERE {Betaald} AND YEARWEEK(r.datum, 1) = YEARWEEK(CURDATE(), 1)");

    public int GetAantalBestellingenVandaag()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COUNT(*)
FROM bestelling b
JOIN reservering r ON r.reservering_id = b.reservering_id
WHERE r.datum = CURDATE()";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public List<(int Betaalwijze, decimal Omzet)> GetOmzetPerBetaalwijze()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT r.betaalwijze, SUM(r.totaal_bedrag)
FROM reservering r
WHERE {Betaald}
GROUP BY r.betaalwijze
ORDER BY r.betaalwijze";
        using var reader = command.ExecuteReader();

        var result = new List<(int, decimal)>();
        while (reader.Read())
        {
            result.Add((reader.GetInt32(0), reader.GetDecimal(1)));
        }
        return result;
    }

    public List<(string Hoofdgroep, decimal Omzet)> GetOmzetPerHoofdgroepDezeWeek()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT COALESCE(g.gerechtnaam, 'Onbekend') AS hoofdgroep, COALESCE(SUM(b.aantal * b.prijs), 0)
FROM bestelling b
JOIN reservering r ON r.reservering_id = b.reservering_id
LEFT JOIN menuitem m ON m.menuitemcode = b.menuitemcode
LEFT JOIN subgerecht s ON s.subgerechtcode = m.subgerechtcode
LEFT JOIN gerecht g ON g.gerechtcode = s.gerechtcode
WHERE {Betaald} AND YEARWEEK(r.datum, 1) = YEARWEEK(CURDATE(), 1)
GROUP BY hoofdgroep
ORDER BY 2 DESC";
        using var reader = command.ExecuteReader();

        var result = new List<(string, decimal)>();
        while (reader.Read())
        {
            result.Add((reader.GetString(0), reader.GetDecimal(1)));
        }
        return result;
    }

    // Alleen weken met omzet; ontbrekende weken vult de service aan met 0.
    public List<(DateOnly WeekStart, decimal Omzet)> GetOmzetPerWeek()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT DATE_SUB(r.datum, INTERVAL WEEKDAY(r.datum) DAY) AS week_start, SUM(r.totaal_bedrag)
FROM reservering r
WHERE {Betaald}
  AND r.datum >= DATE_SUB(DATE_SUB(CURDATE(), INTERVAL WEEKDAY(CURDATE()) DAY), INTERVAL 7 WEEK)
GROUP BY week_start
ORDER BY week_start";
        using var reader = command.ExecuteReader();

        var result = new List<(DateOnly, decimal)>();
        while (reader.Read())
        {
            result.Add((DateOnly.FromDateTime(reader.GetDateTime(0)), reader.GetDecimal(1)));
        }
        return result;
    }

    private decimal ScalarDecimal(string sql)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToDecimal(command.ExecuteScalar());
    }
}