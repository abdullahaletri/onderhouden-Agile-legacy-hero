namespace ExcellentTaste.WinForms;

internal sealed class SoortItem
{
    public SoortItem(string value, string label)
    {
        Value = value;
        Label = label;
    }

    public string Value { get; set; }

    public string Label { get; set; }

    public override string ToString()
    {
        return Label;
    }
}

internal sealed class ComboOption<T>
{
    public ComboOption(string text, T value)
    {
        Text = text;
        Value = value;
    }

    public string Text { get; set; }

    public T Value { get; set; }

    public override string ToString()
    {
        return Text;
    }
}

internal sealed class ReserveringRow
{
    public int Id { get; set; }
    public DateOnly DatumWaarde { get; set; }
    public string Datum { get; set; } = "";
    public string Tijd { get; set; } = "";
    public int Tafel { get; set; }
    public string Klant { get; set; } = "";
    public string Telefoon { get; set; } = "";
    public int Aantal { get; set; }
    public int Kinderen { get; set; }
    public string Opmerkingen { get; set; } = "";
}

internal sealed class BestellingRow
{
    public string MenuItemCode { get; set; } = "";
    public string Naam { get; set; } = "";
    public int Aantal { get; set; }
    public string Prijs { get; set; } = "";
}

internal sealed class OverzichtRow
{
    public int Tafel { get; set; }
    public int Aantal { get; set; }
    public string Item { get; set; } = "";
}

internal sealed class MenuItemRow
{
    public string Code { get; set; } = "";
    public string Omschrijving { get; set; } = "";
    public string Prijs { get; set; } = "";
    public string ValtOnder { get; set; } = "";
    public string ValtOnderCode { get; set; } = "";
}

internal sealed class KlantRow
{
    public int Id { get; set; }
    public string Naam { get; set; } = "";
    public string Telefoon { get; set; } = "";
    public string Email { get; set; } = "";
}

internal sealed class GerechtRow
{
    public string Code { get; set; } = "";
    public string Omschrijving { get; set; } = "";
}

internal sealed class SubgerechtRow
{
    public string Code { get; set; } = "";
    public string Omschrijving { get; set; } = "";
    public string ValtOnder { get; set; } = "";
    public string ValtOnderCode { get; set; } = "";
}
