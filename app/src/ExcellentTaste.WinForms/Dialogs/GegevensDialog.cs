using System.Globalization;
using ExcellentTaste.Core.Models;
using static ExcellentTaste.WinForms.AppCulture;
using static ExcellentTaste.WinForms.WinFormsUi;

namespace ExcellentTaste.WinForms.Dialogs;

internal sealed class GegevensDialog : Form
{
    private readonly TextBox _code = new TextBox();
    private readonly TextBox _omschrijving = new TextBox();
    private readonly TextBox _prijs = new TextBox();
    private readonly TextBox _naam = new TextBox();
    private readonly TextBox _telefoon = new TextBox();
    private readonly TextBox _email = new TextBox();
    private readonly ComboBox _valtOnder = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };

    private GegevensDialog()
    {
        Text = "Gegevens";
        Width = 420;
        Height = 340;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
    }

    public static GegevensDialog ForMenuItem(MenuItem? item, List<Subgerecht> subgerechten)
    {
        GegevensDialog dialog = new GegevensDialog();
        TableLayoutPanel layout = CreateFormLayout();
        dialog.Controls.Add(layout);
        AddRow(layout, "Code", dialog._code);
        AddRow(layout, "Omschrijving", dialog._omschrijving);
        AddRow(layout, "Prijs", dialog._prijs);
        AddRow(layout, "Valt onder", dialog._valtOnder);
        AddButtons(layout);

        foreach (Subgerecht subgerecht in subgerechten)
        {
            dialog._valtOnder.Items.Add(new ComboOption<string>($"{subgerecht.SubgerechtCode} - {subgerecht.SubgerechtNaam}", subgerecht.SubgerechtCode));
        }

        dialog._code.Text = "";
        dialog._omschrijving.Text = "";
        dialog._prijs.Text = 0.ToString("0.00", DutchCulture);
        dialog._code.ReadOnly = false;

        if (item != null)
        {
            dialog._code.Text = item.MenuItemCode;
            dialog._code.ReadOnly = true;
            dialog._omschrijving.Text = item.MenuItemNaam;
            dialog._prijs.Text = item.Prijs.ToString("0.00", DutchCulture);
            SelectComboValue(dialog._valtOnder, item.SubgerechtCode);
        }
        else
        {
            SelectComboValue(dialog._valtOnder, "");
        }

        return dialog;
    }

    public static GegevensDialog ForKlant(Klant? klant)
    {
        GegevensDialog dialog = new GegevensDialog();
        TableLayoutPanel layout = CreateFormLayout();
        dialog.Controls.Add(layout);
        AddRow(layout, "Naam", dialog._naam);
        AddRow(layout, "Telefoon", dialog._telefoon);
        AddRow(layout, "Email", dialog._email);
        AddButtons(layout);

        dialog._naam.Text = "";
        dialog._telefoon.Text = "";
        dialog._email.Text = "";

        if (klant != null)
        {
            dialog._naam.Text = klant.KlantNaam;
            dialog._telefoon.Text = klant.Telefoon;
            dialog._email.Text = klant.Email;
        }

        return dialog;
    }

    public static GegevensDialog ForGerecht(Gerecht? gerecht)
    {
        GegevensDialog dialog = new GegevensDialog();
        TableLayoutPanel layout = CreateFormLayout();
        dialog.Controls.Add(layout);
        AddRow(layout, "Code", dialog._code);
        AddRow(layout, "Omschrijving", dialog._omschrijving);
        AddButtons(layout);

        dialog._code.Text = "";
        dialog._omschrijving.Text = "";
        dialog._code.ReadOnly = false;

        if (gerecht != null)
        {
            dialog._code.Text = gerecht.GerechtCode;
            dialog._code.ReadOnly = true;
            dialog._omschrijving.Text = gerecht.GerechtNaam;
        }

        return dialog;
    }

    public static GegevensDialog ForSubgerecht(Subgerecht? subgerecht, List<Gerecht> gerechten)
    {
        GegevensDialog dialog = new GegevensDialog();
        TableLayoutPanel layout = CreateFormLayout();
        dialog.Controls.Add(layout);
        AddRow(layout, "Code", dialog._code);
        AddRow(layout, "Omschrijving", dialog._omschrijving);
        AddRow(layout, "Valt onder", dialog._valtOnder);
        AddButtons(layout);

        foreach (Gerecht gerecht in gerechten)
        {
            dialog._valtOnder.Items.Add(new ComboOption<string>($"{gerecht.GerechtCode} - {gerecht.GerechtNaam}", gerecht.GerechtCode));
        }

        dialog._code.Text = "";
        dialog._omschrijving.Text = "";
        dialog._code.ReadOnly = false;

        if (subgerecht != null)
        {
            dialog._code.Text = subgerecht.SubgerechtCode;
            dialog._code.ReadOnly = true;
            dialog._omschrijving.Text = subgerecht.SubgerechtNaam;
            SelectComboValue(dialog._valtOnder, subgerecht.GerechtCode);
        }
        else
        {
            SelectComboValue(dialog._valtOnder, "");
        }

        return dialog;
    }

    public MenuItem ToMenuItem()
    {
        MenuItem item = new MenuItem();
        item.MenuItemCode = _code.Text;
        item.MenuItemNaam = _omschrijving.Text;
        item.Prijs = ParsePrijs(_prijs.Text);
        item.SubgerechtCode = ((ComboOption<string>)_valtOnder.SelectedItem!).Value;
        return item;
    }

    public Klant ToKlant()
    {
        Klant klant = new Klant();
        klant.KlantNaam = _naam.Text;
        klant.Telefoon = _telefoon.Text;
        klant.Email = _email.Text;
        klant.Status = 1;
        return klant;
    }

    public Gerecht ToGerecht()
    {
        Gerecht gerecht = new Gerecht();
        gerecht.GerechtCode = _code.Text;
        gerecht.GerechtNaam = _omschrijving.Text;
        return gerecht;
    }

    public Subgerecht ToSubgerecht()
    {
        Subgerecht subgerecht = new Subgerecht();
        subgerecht.SubgerechtCode = _code.Text;
        subgerecht.SubgerechtNaam = _omschrijving.Text;
        subgerecht.GerechtCode = ((ComboOption<string>)_valtOnder.SelectedItem!).Value;
        return subgerecht;
    }

    private static decimal ParsePrijs(string value)
    {
        decimal dutchValue;
        if (decimal.TryParse(value, NumberStyles.Number, DutchCulture, out dutchValue))
        {
            return dutchValue;
        }

        return decimal.Parse(value, NumberStyles.Number, CultureInfo.InvariantCulture);
    }
}
