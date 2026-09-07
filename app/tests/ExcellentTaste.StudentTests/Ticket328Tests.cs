using Xunit;

namespace ExcellentTaste.StudentTests;

public class Ticket328Tests
{
    [Fact]
    public void GegevensVerwijderen_VraagtEerstOmBevestiging()
    {
        string source = TestProjectFiles.Read("src/ExcellentTaste.WinForms/Form1.cs");

        Assert.Contains("MessageBox.Show(this, \"Gegevens verwijderen?\", \"Gegevens\", MessageBoxButtons.YesNo", source);
        Assert.Contains("!= DialogResult.Yes", source);
    }
}
