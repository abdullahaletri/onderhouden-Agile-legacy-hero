using ExcellentTaste.Core.Data;
using ExcellentTaste.Core.Models;

namespace ExcellentTaste.Core.Repositories;

public class KlantRepository : RepositoryBase
{
    private readonly DatabaseConnection _database;

    public KlantRepository(DatabaseConnection database)
    {
        _database = database;
    }

    public List<Klant> GetAll()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT klant_id, klantnaam, telefoon, email, status FROM klant ORDER BY klantnaam";
        using var reader = command.ExecuteReader();

        var klanten = new List<Klant>();
        while (reader.Read())
        {
            klanten.Add(new Klant
            {
                KlantId = Int(reader, "klant_id"),
                KlantNaam = Text(reader, "klantnaam"),
                Telefoon = Text(reader, "telefoon"),
                Email = Text(reader, "email"),
                Status = Int(reader, "status")
            });
        }

        return klanten;
    }

    public Klant? GetById(int id)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT klant_id, klantnaam, telefoon, email, status FROM klant WHERE klant_id = @id";
        Add(command, "@id", id);
        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return new Klant
        {
            KlantId = Int(reader, "klant_id"),
            KlantNaam = Text(reader, "klantnaam"),
            Telefoon = Text(reader, "telefoon"),
            Email = Text(reader, "email"),
            Status = Int(reader, "status")
        };
    }

    public int Add(Klant klant)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
INSERT INTO klant (klantnaam, telefoon, email, status)
VALUES (@naam, @telefoon, @email, @status);
SELECT LAST_INSERT_ID();
""";
        Add(command, "@naam", NullIfEmpty(klant.KlantNaam));
        Add(command, "@telefoon", NullIfEmpty(klant.Telefoon));
        Add(command, "@email", NullIfEmpty(klant.Email));
        Add(command, "@status", klant.Status);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(int id, Klant klant)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
UPDATE klant
SET klantnaam = @naam, telefoon = @telefoon, email = @email, status = @status
WHERE klant_id = @id
""";
        Add(command, "@id", id);
        Add(command, "@naam", NullIfEmpty(klant.KlantNaam));
        Add(command, "@telefoon", NullIfEmpty(klant.Telefoon));
        Add(command, "@email", NullIfEmpty(klant.Email));
        Add(command, "@status", klant.Status);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM klant WHERE klant_id = @id";
        Add(command, "@id", id);
        command.ExecuteNonQuery();
    }

    private static object? NullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
