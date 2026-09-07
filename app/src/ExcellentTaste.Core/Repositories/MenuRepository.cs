using ExcellentTaste.Core.Data;
using ExcellentTaste.Core.Models;

namespace ExcellentTaste.Core.Repositories;

public class MenuRepository : RepositoryBase
{
    private readonly DatabaseConnection _database;

    public MenuRepository(DatabaseConnection database)
    {
        _database = database;
    }

    public List<MenuItem> GetMenuItems(string? soort = null)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
SELECT m.subgerechtcode, m.menuitemcode, m.menuitemnaam, m.prijs
FROM menuitem m
LEFT JOIN subgerecht s ON m.subgerechtcode = s.subgerechtcode
WHERE (@soort IS NULL)
   OR (@soort = 'drinken' AND s.gerechtcode = 'drk')
   OR (@soort = 'eten' AND s.gerechtcode <> 'drk')
ORDER BY m.menuitemcode
""";
        Add(command, "@soort", soort);
        using var reader = command.ExecuteReader();

        var items = new List<MenuItem>();
        while (reader.Read())
        {
            items.Add(new MenuItem
            {
                SubgerechtCode = Text(reader, "subgerechtcode"),
                MenuItemCode = Text(reader, "menuitemcode"),
                MenuItemNaam = Text(reader, "menuitemnaam"),
                Prijs = Decimal(reader, "prijs")
            });
        }

        return items;
    }

    public List<Gerecht> GetGerechten()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT gerechtcode, gerechtnaam FROM gerecht ORDER BY gerechtcode";
        using var reader = command.ExecuteReader();

        var gerechten = new List<Gerecht>();
        while (reader.Read())
        {
            gerechten.Add(new Gerecht
            {
                GerechtCode = Text(reader, "gerechtcode"),
                GerechtNaam = Text(reader, "gerechtnaam")
            });
        }

        return gerechten;
    }

    public List<Subgerecht> GetSubgerechten()
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT gerechtcode, subgerechtcode, subgerechtnaam FROM subgerecht ORDER BY subgerechtcode";
        using var reader = command.ExecuteReader();

        var subgerechten = new List<Subgerecht>();
        while (reader.Read())
        {
            subgerechten.Add(new Subgerecht
            {
                GerechtCode = Text(reader, "gerechtcode"),
                SubgerechtCode = Text(reader, "subgerechtcode"),
                SubgerechtNaam = Text(reader, "subgerechtnaam")
            });
        }

        return subgerechten;
    }

    public MenuItem? GetByCode(string code)
    {
        return GetMenuItems().FirstOrDefault(item => item.MenuItemCode.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public Gerecht? GetGerechtByCode(string code)
    {
        return GetGerechten().FirstOrDefault(gerecht => gerecht.GerechtCode.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public Subgerecht? GetSubgerechtByCode(string code)
    {
        return GetSubgerechten().FirstOrDefault(subgerecht => subgerecht.SubgerechtCode.Equals(code, StringComparison.OrdinalIgnoreCase));
    }

    public void AddMenuItem(MenuItem item)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
INSERT INTO menuitem (subgerechtcode, menuitemcode, menuitemnaam, prijs)
VALUES (@subgerechtcode, @menuitemcode, @menuitemnaam, @prijs)
""";
        AddMenuItemParameters(command, item);
        command.ExecuteNonQuery();
    }

    public void UpdateMenuItem(string code, MenuItem item)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
UPDATE menuitem
SET menuitemnaam = @menuitemnaam, prijs = @prijs, subgerechtcode = @subgerechtcode
WHERE menuitemcode = @code
""";
        Add(command, "@code", code);
        AddMenuItemParameters(command, item);
        command.ExecuteNonQuery();
    }

    public void DeleteMenuItem(string code)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM menuitem WHERE menuitemcode = @code";
        Add(command, "@code", code);
        command.ExecuteNonQuery();
    }

    public void AddGerecht(Gerecht gerecht)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO gerecht (gerechtcode, gerechtnaam) VALUES (@code, @naam)";
        Add(command, "@code", gerecht.GerechtCode);
        Add(command, "@naam", gerecht.GerechtNaam);
        command.ExecuteNonQuery();
    }

    public void UpdateGerecht(string code, Gerecht gerecht)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE gerecht SET gerechtnaam = @naam WHERE gerechtcode = @code";
        Add(command, "@code", code);
        Add(command, "@naam", gerecht.GerechtNaam);
        command.ExecuteNonQuery();
    }

    public void DeleteGerecht(string code)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM gerecht WHERE gerechtcode = @code";
        Add(command, "@code", code);
        command.ExecuteNonQuery();
    }

    public void AddSubgerecht(Subgerecht subgerecht)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
INSERT INTO subgerecht (gerechtcode, subgerechtcode, subgerechtnaam)
VALUES (@gerechtcode, @subgerechtcode, @subgerechtnaam)
""";
        AddSubgerechtParameters(command, subgerecht);
        command.ExecuteNonQuery();
    }

    public void UpdateSubgerecht(string code, Subgerecht subgerecht)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
UPDATE subgerecht
SET subgerechtnaam = @subgerechtnaam, gerechtcode = @gerechtcode
WHERE subgerechtcode = @code
""";
        Add(command, "@code", code);
        AddSubgerechtParameters(command, subgerecht);
        command.ExecuteNonQuery();
    }

    public void DeleteSubgerecht(string code)
    {
        using var connection = _database.OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM subgerecht WHERE subgerechtcode = @code";
        Add(command, "@code", code);
        command.ExecuteNonQuery();
    }

    private static void AddMenuItemParameters(MySqlConnector.MySqlCommand command, MenuItem item)
    {
        Add(command, "@subgerechtcode", item.SubgerechtCode);
        Add(command, "@menuitemcode", item.MenuItemCode);
        Add(command, "@menuitemnaam", item.MenuItemNaam);
        Add(command, "@prijs", item.Prijs);
    }

    private static void AddSubgerechtParameters(MySqlConnector.MySqlCommand command, Subgerecht subgerecht)
    {
        Add(command, "@gerechtcode", subgerecht.GerechtCode);
        Add(command, "@subgerechtcode", subgerecht.SubgerechtCode);
        Add(command, "@subgerechtnaam", subgerecht.SubgerechtNaam);
    }
}
