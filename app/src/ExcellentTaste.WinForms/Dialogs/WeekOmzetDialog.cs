using System.ComponentModel;
using ExcellentTaste.Core.Models;
using ExcellentTaste.WinForms.Api;
using static ExcellentTaste.WinForms.AppCulture;
using static ExcellentTaste.WinForms.WinFormsUi;

namespace ExcellentTaste.WinForms.Dialogs;

internal sealed class WeekOmzetDialog : Form
{
    private readonly ExcellentTasteApiClient _api;
    private readonly DataGridView _grid = CreateGrid();
    private readonly ComboBox _jaar = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
    private readonly ComboBox _week = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
    private readonly Label _melding = new Label { AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) };
    private bool _filtersVullen;

    public WeekOmzetDialog(ExcellentTasteApiClient api)
    {
        _api = api;

        Text = "Weekoverzicht omzet";
        Width = 800;
        Height = 600;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;

        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 3
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        Controls.Add(layout);

        FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        Button refresh = CreateButton("Vernieuwen", 112);
        Button close = CreateButton("Sluiten");
        toolbar.Controls.AddRange(new Control[]
        {
            new Label { Text = "Jaar:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) },
            _jaar,
            new Label { Text = "Week:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) },
            _week,
            refresh,
            close
        });

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_grid, 0, 1);
        layout.Controls.Add(_melding, 0, 2);

        _jaar.SelectedIndexChanged += delegate { if (!_filtersVullen) TryRunApiAction(LoadOmzet); };
        _week.SelectedIndexChanged += delegate { if (!_filtersVullen) TryRunApiAction(LoadOmzet); };
        refresh.Click += delegate { TryRunApiAction(LoadOmzet); };
        close.Click += delegate { Close(); };

        TryRunApiAction(() =>
        {
            FillFilters();
            LoadOmzet();
        });
    }

    private void FillFilters()
    {
        _filtersVullen = true;

        // De jaren komen uit de omzet zelf, zodat je alleen jaren met omzet kunt kiezen.
        List<int> jaren = _api.GetWeekOmzet()
            .Select(regel => regel.Jaar)
            .Distinct()
            .OrderByDescending(jaar => jaar)
            .ToList();

        _jaar.Items.Clear();
        _jaar.Items.Add(new ComboOption<int?>("Alle jaren", null));
        foreach (int jaar in jaren)
        {
            _jaar.Items.Add(new ComboOption<int?>(jaar.ToString(), jaar));
        }

        _week.Items.Clear();
        _week.Items.Add(new ComboOption<int?>("Alle weken", null));
        for (int week = 1; week <= 53; week++)
        {
            _week.Items.Add(new ComboOption<int?>($"Week {week}", week));
        }

        _jaar.SelectedIndex = 0;
        _week.SelectedIndex = 0;
        _filtersVullen = false;
    }

    private void LoadOmzet()
    {
        int? jaar = (_jaar.SelectedItem as ComboOption<int?>)?.Value;
        int? week = (_week.SelectedItem as ComboOption<int?>)?.Value;
        List<WeekOmzetRegel> regels = _api.GetWeekOmzet(jaar, week);

        List<WeekOmzetRow> rows = new List<WeekOmzetRow>();
        foreach (WeekOmzetRegel regel in regels)
        {
            WeekOmzetRow row = new WeekOmzetRow();
            row.Week = $"{regel.Jaar}-{regel.Week:00}";
            row.Hoofdgroep = regel.Gerecht;
            row.Omzet = regel.Omzet.ToString("N2", DutchCulture);
            rows.Add(row);
        }

        _grid.DataSource = new BindingList<WeekOmzetRow>(rows);
        if (_grid.Columns[nameof(WeekOmzetRow.Omzet)] != null)
        {
            _grid.Columns[nameof(WeekOmzetRow.Omzet)]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        if (regels.Count == 0)
        {
            _melding.Text = "Geen omzet gevonden voor deze selectie.";
        }
        else
        {
            decimal totaal = regels.Sum(regel => regel.Omzet);
            _melding.Text = "Totaal: " + totaal.ToString("N2", DutchCulture);
        }
    }

    private void TryRunApiAction(Action action)
    {
        try
        {
            action();
        }
        catch (ApiUnavailableException)
        {
            MessageBox.Show(this, "De API is niet bereikbaar.\r\n\r\nNeem contact op met de administrator.", "API niet bereikbaar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed class WeekOmzetRow
    {
        public string Week { get; set; } = "";
        public string Hoofdgroep { get; set; } = "";
        public string Omzet { get; set; } = "";
    }
}
