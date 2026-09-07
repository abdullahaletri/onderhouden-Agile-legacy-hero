using System.Globalization;
using ExcellentTaste.Core.Models;
using static ExcellentTaste.WinForms.AppConstants;
using static ExcellentTaste.WinForms.WinFormsUi;

namespace ExcellentTaste.WinForms.Dialogs;

internal sealed class ReserveringDialog : Form
{
    private readonly ComboBox _klant = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _nieuweNaam = new TextBox();
    private readonly TextBox _nieuweTelefoon = new TextBox();
    private readonly TextBox _nieuweEmail = new TextBox();
    private readonly NumericUpDown _tafel = new NumericUpDown { Minimum = 1, Maximum = 999 };
    private readonly DateTimePicker _datum = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd-MM-yyyy" };
    private readonly TextBox _tijd = new TextBox();
    private readonly NumericUpDown _aantal = new NumericUpDown { Minimum = 1, Maximum = 999 };
    private readonly NumericUpDown _kinderen = new NumericUpDown { Minimum = 0, Maximum = 999 };
    private readonly TextBox _opmerkingen = new TextBox();

    public ReserveringDialog(List<Klant> klanten, Reservering reservering)
    {
        if (reservering.ReserveringId == 0)
        {
            Text = "Nieuwe reservering";
        }
        else
        {
            Text = "Reservering wijzigen";
        }
        Width = 460;
        Height = 520;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        TableLayoutPanel layout = CreateFormLayout();
        Controls.Add(layout);

        _klant.DisplayMember = "Text";
        _klant.ValueMember = "Value";
        _klant.Items.Add(new ComboOption<int>("--Nieuwe klant--", NieuweKlantId));
        foreach (Klant klant in klanten)
        {
            if (klant.Status == 1)
            {
                _klant.Items.Add(new ComboOption<int>(klant.KlantNaam, klant.KlantId));
            }
        }

        AddRow(layout, "Klant", _klant);
        AddNieuweKlantPanel(layout);
        AddRow(layout, "Tafel", _tafel);
        AddRow(layout, "Datum", _datum);
        AddRow(layout, "Tijd", _tijd);
        AddRow(layout, "Volwassenen", _aantal);
        AddRow(layout, "Kinderen", _kinderen);
        AddRow(layout, "Opmerkingen", _opmerkingen);
        AddButtons(layout);

        _klant.SelectedIndexChanged += delegate { ToggleNieuweKlant(); };
        SelectKlant(reservering.KlantId);
        _nieuweNaam.Text = reservering.KlantNaam;
        _nieuweTelefoon.Text = reservering.Telefoon;
        _tafel.Value = Math.Max(_tafel.Minimum, reservering.Tafel);
        if (reservering.Datum == DateOnly.MinValue)
        {
            _datum.Value = DateTime.Today.AddDays(-1);
        }
        else
        {
            _datum.Value = reservering.Datum.ToDateTime(TimeOnly.MinValue);
        }

        if (reservering.Tijd == TimeOnly.MinValue)
        {
            _tijd.Text = "18:00";
        }
        else
        {
            _tijd.Text = reservering.Tijd.ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        _aantal.Value = Math.Max(_aantal.Minimum, reservering.Aantal);
        _kinderen.Value = Math.Max(_kinderen.Minimum, reservering.AantalKinderen);
        _opmerkingen.Text = "";

        if (reservering.Opmerkingen != null)
        {
            _opmerkingen.Text = reservering.Opmerkingen;
        }

        ToggleNieuweKlant();
    }

    private void AddNieuweKlantPanel(TableLayoutPanel layout)
    {
        Panel panel = new Panel
        {
            Dock = DockStyle.None,
            Width = 185,
            Height = 96,
            Margin = new Padding(13, 3, 0, 3)
        };
        panel.Paint += delegate(object? sender, PaintEventArgs e)
        {
            using Pen border = new Pen(Color.DimGray);
            e.Graphics.DrawRectangle(border, 0, 0, panel.Width - 1, panel.Height - 1);
        };

        TableLayoutPanel klantLayout = new TableLayoutPanel
        {
            Location = new Point(0, 0),
            Width = 320,
            Height = 96,
            ColumnCount = 2,
            RowCount = 3
        };
        klantLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
        klantLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));

        AddNieuweKlantRow(klantLayout, 0, "Naam", _nieuweNaam);
        AddNieuweKlantRow(klantLayout, 1, "Telefoon", _nieuweTelefoon);
        AddNieuweKlantRow(klantLayout, 2, "Email", _nieuweEmail);
        panel.Controls.Add(klantLayout);

        int row = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
        layout.Controls.Add(new Label { Text = "Nieuwe klant", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        layout.Controls.Add(panel, 1, row);
    }

    private static void AddNieuweKlantRow(TableLayoutPanel layout, int row, string label, TextBox textBox)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
        layout.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        textBox.Dock = DockStyle.Fill;
        layout.Controls.Add(textBox, 1, row);
    }

    public int SelectedKlantId
    {
        get
        {
            ComboOption<int> selected = (ComboOption<int>)_klant.SelectedItem!;
            return selected.Value;
        }
    }

    public Klant NieuweKlant
    {
        get
        {
            Klant klant = new Klant();
            klant.KlantNaam = _nieuweNaam.Text;
            klant.Telefoon = _nieuweTelefoon.Text;
            klant.Email = _nieuweEmail.Text;
            klant.Status = 1;
            return klant;
        }
    }

    public Reservering ToReservering(int klantId)
    {
        TimeOnly.TryParseExact(_tijd.Text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var tijd);

        return new Reservering
        {
            KlantId = klantId,
            Tafel = (int)_tafel.Value,
            Datum = DateOnly.FromDateTime(_datum.Value),
            Tijd = tijd,
            Aantal = (int)_aantal.Value,
            AantalKinderen = (int)_kinderen.Value,
            Opmerkingen = _opmerkingen.Text,
            Status = 1
        };
    }

    private void ToggleNieuweKlant()
    {
        bool enabled = SelectedKlantId == NieuweKlantId;
        _nieuweNaam.Enabled = enabled;
        _nieuweTelefoon.Enabled = enabled;
        _nieuweEmail.Enabled = enabled;
    }

    private void SelectKlant(int klantId)
    {
        foreach (ComboOption<int> item in _klant.Items)
        {
            if (item.Value == klantId)
            {
                _klant.SelectedItem = item;
                return;
            }
        }

        if (_klant.Items.Count > 0)
        {
            _klant.SelectedIndex = 0;
        }
    }
}
