namespace ExcellentTaste.Core.Data;

public class DatabaseUnavailableException : Exception
{
    public DatabaseUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
