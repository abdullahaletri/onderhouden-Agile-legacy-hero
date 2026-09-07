using System.Globalization;
using MySqlConnector;

namespace ExcellentTaste.Core.Repositories;

public abstract class RepositoryBase
{
    protected static string Text(MySqlDataReader reader, string name)
    {
        var value = reader[name];
        return value == DBNull.Value ? "" : value?.ToString() ?? "";
    }

    protected static int Int(MySqlDataReader reader, string name) => Convert.ToInt32(reader[name]);

    protected static decimal Decimal(MySqlDataReader reader, string name) => Convert.ToDecimal(reader[name]);

    protected static DateOnly Date(MySqlDataReader reader, string name)
    {
        int ordinal = reader.GetOrdinal(name);
        if (reader.IsDBNull(ordinal))
        {
            return DateOnly.MinValue;
        }

        try
        {
            return DateOnly.FromDateTime(reader.GetDateTime(ordinal));
        }
        catch (Exception ex) when (ex is InvalidCastException or MySqlConversionException)
        {
        }

        var value = reader[name];
        if (value == DBNull.Value)
        {
            return DateOnly.MinValue;
        }

        if (value is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }

        string text = value.ToString() ?? "";
        string[] formats =
        {
            "yyyy-MM-dd",
            "yyyy-M-d",
            "dd-MM-yyyy",
            "d-M-yyyy",
            "M/d/yyyy h:mm:ss tt",
            "M/d/yyyy hh:mm:ss tt",
            "MM/dd/yyyy HH:mm:ss",
            "M/d/yyyy HH:mm:ss"
        };

        return DateOnly.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : DateOnly.MinValue;
    }

    protected static TimeOnly Time(MySqlDataReader reader, string name)
    {
        int ordinal = reader.GetOrdinal(name);
        if (reader.IsDBNull(ordinal))
        {
            return TimeOnly.MinValue;
        }

        try
        {
            return TimeOnly.FromDateTime(reader.GetDateTime(ordinal));
        }
        catch (Exception ex) when (ex is InvalidCastException or MySqlConversionException)
        {
        }

        var value = reader[name];
        if (value == DBNull.Value)
        {
            return TimeOnly.MinValue;
        }

        if (value is TimeSpan timeSpan)
        {
            return TimeOnly.FromTimeSpan(timeSpan);
        }

        if (value is DateTime dateTime)
        {
            return TimeOnly.FromDateTime(dateTime);
        }

        string text = value.ToString() ?? "";
        string[] formats =
        {
            "HH:mm:ss",
            "H:mm:ss",
            "HH:mm",
            "H:mm"
        };

        return TimeOnly.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)
            ? time
            : TimeOnly.MinValue;
    }

    protected static void Add(MySqlCommand command, string name, object? value)
    {
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }
}
