using ExcellentTaste.Core.Models;
using ExcellentTaste.Web.ViewModels;
using ExcellentTaste.WinForms.Dialogs;
using Xunit;

namespace ExcellentTaste.StudentTests;

public class Ticket329Tests
{
    [Fact]
    public void WinForms_NieuweReservering_GebruiktVandaagAlsStandaarddatum()
    {
        using var dialog = new ReserveringDialog([], new Reservering());

        Reservering reservering = dialog.ToReservering(-1);

        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), reservering.Datum);
    }

    [Fact]
    public void Web_NieuweReservering_GebruiktVandaagAlsStandaarddatum()
    {
        var model = new ReserveringFormViewModel();

        Assert.Equal(DateOnly.FromDateTime(DateTime.Today).ToString("dd-MM-yyyy"), model.Datum);
    }
}
