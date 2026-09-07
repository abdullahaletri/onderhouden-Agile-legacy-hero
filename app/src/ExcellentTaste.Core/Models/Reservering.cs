namespace ExcellentTaste.Core.Models;

public class Reservering
{
    public int ReserveringId { get; set; }
    public int Tafel { get; set; }
    public DateOnly Datum { get; set; }
    public TimeOnly Tijd { get; set; }
    public int KlantId { get; set; }
    public string KlantNaam { get; set; } = "";
    public string Telefoon { get; set; } = "";
    public int Aantal { get; set; }
    public int Status { get; set; } = 1;
    public DateTime DatumToegevoegd { get; set; } = DateTime.Now;
    public int AantalKinderen { get; set; }
    public string? Opmerkingen { get; set; }
}
