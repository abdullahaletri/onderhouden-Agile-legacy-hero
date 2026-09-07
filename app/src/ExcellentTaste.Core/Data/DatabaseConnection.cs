using MySqlConnector;

namespace ExcellentTaste.Core.Data;

public class DatabaseConnection
{
    private readonly string _connectionString;

    public DatabaseConnection(string? connectionString)
    {
        _connectionString = connectionString ?? "";
    }

    public MySqlConnection OpenConnection()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new DatabaseUnavailableException(
                "De database is niet bereikbaar. Neem contact op met de administrator.",
                new InvalidOperationException("Connection string 'ExcellentTaste' ontbreekt. Geef een MySQL-verbinding mee."));
        }

        var connection = new MySqlConnection(_connectionString);
        try
        {
            connection.Open();
            return connection;
        }
        catch (Exception ex) when (ex is MySqlException or TimeoutException)
        {
            connection.Dispose();
            throw new DatabaseUnavailableException("De database is niet bereikbaar. Neem contact op met de administrator.", ex);
        }
    }

    public void Initialize()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM klant";
        command.ExecuteScalar();
    }
}
