namespace ExcellentTaste.WinForms.Api;

internal sealed class ApiUnavailableException : Exception
{
    public ApiUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
