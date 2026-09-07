using Xunit;

namespace ExcellentTaste.StudentTests;

public class Ticket322Tests
{
    [Fact]
    public void NieuweReservering_ControleertOfTafelAlBezetIs()
    {
        string source = TestProjectFiles.Read("src/ExcellentTaste.Core/Services/ReserveringService.cs");

        Assert.Contains("_reserveringen.IsTafelBezet(reservering.Tafel, reservering.Datum, reservering.Tijd)", source);
    }

    [Fact]
    public void BestaandeReservering_ControleertOfTafelAlBezetIsMaarNegeertZichzelf()
    {
        string source = TestProjectFiles.Read("src/ExcellentTaste.Core/Services/ReserveringService.cs");

        Assert.Contains("_reserveringen.IsTafelBezet(reservering.Tafel, reservering.Datum, reservering.Tijd, id)", source);
    }
}
