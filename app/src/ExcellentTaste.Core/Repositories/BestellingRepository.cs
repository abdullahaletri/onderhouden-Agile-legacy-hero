using ExcellentTaste.Core.Data;
using ExcellentTaste.Core.Models;

namespace ExcellentTaste.Core.Repositories;

public class BestellingRepository : RepositoryBase
{
    private readonly DatabaseConnection _database;

    public BestellingRepository(DatabaseConnection database)
    {
        _database = database;
    }

    public List<Bestelling> GetByReservering(int reserveringId)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
SELECT b.bestelling_id, b.reservering_id, b.tafel, b.datum, b.tijd, b.menuitemcode,
       b.aantal, b.prijs, m.menuitemnaam
FROM bestelling b
LEFT JOIN menuitem m ON b.menuitemcode = m.menuitemcode
WHERE b.reservering_id = @reserveringId
ORDER BY b.bestelling_id
""";
        Add(command, "@reserveringId", reserveringId);
        using var reader = command.ExecuteReader();

        var bestellingen = new List<Bestelling>();
        while (reader.Read())
        {
            bestellingen.Add(new Bestelling
            {
                BestellingId = Int(reader, "bestelling_id"),
                ReserveringId = Int(reader, "reservering_id"),
                Tafel = Int(reader, "tafel"),
                Datum = Date(reader, "datum"),
                Tijd = Time(reader, "tijd"),
                MenuItemCode = Text(reader, "menuitemcode"),
                MenuItemNaam = Text(reader, "menuitemnaam"),
                Aantal = Int(reader, "aantal"),
                Prijs = Decimal(reader, "prijs")
            });
        }

        return bestellingen;
    }

    public void AddItem(int reserveringId, int tafel, string menuItemCode, decimal prijs)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        // Legacy issue: hetzelfde menuitem wordt opnieuw toegevoegd in plaats van aantal + 1.
        command.CommandText = """
INSERT INTO bestelling (reservering_id, tafel, datum, tijd, menuitemcode, aantal, prijs)
VALUES (@reserveringId, @tafel, @datum, @tijd, @menuItemCode, 1, @prijs)
""";
        Add(command, "@reserveringId", reserveringId);
        Add(command, "@tafel", tafel);
        Add(command, "@datum", DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"));
        Add(command, "@tijd", TimeOnly.FromDateTime(DateTime.Now).ToString("HH:mm"));
        Add(command, "@menuItemCode", menuItemCode);
        Add(command, "@prijs", prijs);
        command.ExecuteNonQuery();
    }

    public void ChangeAantal(int reserveringId, string menuItemCode, int stap)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
UPDATE bestelling
SET aantal = aantal + @stap
WHERE bestelling_id = (
    SELECT bestelling_id FROM bestelling
    WHERE reservering_id = @reserveringId AND menuitemcode = @menuItemCode
    ORDER BY bestelling_id LIMIT 1
)
""";
        Add(command, "@stap", stap);
        Add(command, "@reserveringId", reserveringId);
        Add(command, "@menuItemCode", menuItemCode);
        command.ExecuteNonQuery();
    }

    public void DeleteItem(int reserveringId, string menuItemCode)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
DELETE FROM bestelling
WHERE reservering_id = @reserveringId AND menuitemcode = @menuItemCode
""";
        Add(command, "@reserveringId", reserveringId);
        Add(command, "@menuItemCode", menuItemCode);
        command.ExecuteNonQuery();
    }
}
