using ExcellentTaste.Core.Data;
using ExcellentTaste.Core.Models;

namespace ExcellentTaste.Core.Repositories;

public class ReserveringRepository : RepositoryBase
{
    private readonly DatabaseConnection _database;

    public ReserveringRepository(DatabaseConnection database)
    {
        _database = database;
    }

    public List<Reservering> GetAll()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        // Legacy issue: geannuleerde reserveringen worden hier nog niet weggefilterd.
        command.CommandText = """
SELECT r.reservering_id, r.tafel, r.datum, r.tijd, r.klant_id, r.aantal, r.status,
       r.datum_toegevoegd, r.aantal_k, r.opmerkingen,
       k.klantnaam, k.telefoon
FROM reservering r
LEFT JOIN klant k ON r.klant_id = k.klant_id
ORDER BY r.datum, r.tijd, r.tafel
""";
        using var reader = command.ExecuteReader();

        var reserveringen = new List<Reservering>();
        while (reader.Read())
        {
            reserveringen.Add(Map(reader));
        }

        return reserveringen;
    }

    public Reservering? GetById(int id)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
SELECT r.reservering_id, r.tafel, r.datum, r.tijd, r.klant_id, r.aantal, r.status,
       r.datum_toegevoegd, r.aantal_k, r.opmerkingen,
       k.klantnaam, k.telefoon
FROM reservering r
LEFT JOIN klant k ON r.klant_id = k.klant_id
WHERE r.reservering_id = @id
""";
        Add(command, "@id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public bool IsTafelBezet(int tafel, DateOnly datum, TimeOnly tijd, int? uitgezonderdeReserveringId = null)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
SELECT COUNT(*) FROM reservering
WHERE tafel = @tafel AND datum = @datum AND tijd = @tijd
  AND (@uitgezonderdeReserveringId IS NULL OR reservering_id <> @uitgezonderdeReserveringId)
""";
        Add(command, "@tafel", tafel);
        Add(command, "@datum", datum.ToString("yyyy-MM-dd"));
        Add(command, "@tijd", tijd.ToString("HH:mm"));
        Add(command, "@uitgezonderdeReserveringId", uitgezonderdeReserveringId);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public int Add(Reservering reservering)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        string datum = reservering.Datum == DateOnly.MinValue ? "" : $"'{reservering.Datum:yyyy-MM-dd}'";
        string tijd = reservering.Tijd == TimeOnly.MinValue ? "" : $"'{reservering.Tijd:HH\\:mm}'";
        string aantal = reservering.Aantal == 0 ? "" : reservering.Aantal.ToString();
        command.CommandText =
            "INSERT INTO reservering (tafel, datum, tijd, klant_id, aantal, status, aantal_k, opmerkingen) VALUES (" +
            reservering.Tafel + ", " +
            datum + ", " +
            tijd + ", " +
            reservering.KlantId + ", " +
            aantal + ", " +
            reservering.Status + ", " +
            reservering.AantalKinderen + ", '" +
            EscapeSqlText(reservering.Opmerkingen ?? "") + "'); " +
            "SELECT LAST_INSERT_ID();";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(int id, Reservering reservering)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
UPDATE reservering
SET tafel = @tafel, datum = @datum, tijd = @tijd, klant_id = @klantId, aantal = @aantal,
    status = @status, aantal_k = @aantalKinderen, opmerkingen = @opmerkingen
WHERE reservering_id = @id
""";
        Add(command, "@id", id);
        AddParameters(command, reservering);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        // Legacy issue: dit verwijdert echt, terwijl de database ook een statusveld heeft.
        command.CommandText = "DELETE FROM reservering WHERE reservering_id = -1";
        Add(command, "@id", id);
        command.ExecuteNonQuery();
    }

    private static void AddParameters(MySqlConnector.MySqlCommand command, Reservering reservering)
    {
        Add(command, "@tafel", reservering.Tafel);
        Add(command, "@datum", reservering.Datum.ToString("yyyy-MM-dd"));
        Add(command, "@tijd", reservering.Tijd.ToString("HH:mm"));
        Add(command, "@klantId", reservering.KlantId);
        Add(command, "@aantal", reservering.Aantal);
        Add(command, "@status", reservering.Status);
        Add(command, "@aantalKinderen", reservering.AantalKinderen);
        Add(command, "@opmerkingen", reservering.Opmerkingen);
    }

    private static string EscapeSqlText(string value)
    {
        return value.Replace("'", "''");
    }

    private static Reservering Map(MySqlConnector.MySqlDataReader reader)
    {
        return new Reservering
        {
            ReserveringId = Int(reader, "reservering_id"),
            Tafel = Int(reader, "tafel"),
            Datum = Date(reader, "datum"),
            Tijd = Time(reader, "tijd"),
            KlantId = Int(reader, "klant_id"),
            Aantal = Int(reader, "aantal"),
            Status = Int(reader, "status"),
            AantalKinderen = Int(reader, "aantal_k"),
            Opmerkingen = Text(reader, "opmerkingen"),
            KlantNaam = Text(reader, "klantnaam"),
            Telefoon = Text(reader, "telefoon")
        };
    }
}
