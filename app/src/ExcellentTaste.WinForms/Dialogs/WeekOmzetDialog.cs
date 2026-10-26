using System.ComponentModel;
using ExcellentTaste.Core.Models;
using ExcellentTaste.WinForms.Api;
using ScottPlot.WinForms;
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
    private readonly FormsPlot _perWeek = CreatePlot();
    private readonly FormsPlot _perGerecht = CreatePlot();
    private readonly FormsPlot _gestapeld = CreatePlot();
    private readonly Label _totaal;
    private readonly Label _gemiddeld;
    private readonly Label _besteWeek;
    private readonly Label _topGerecht;
    private bool _filtersVullen;

    public WeekOmzetDialog(ExcellentTasteApiClient api)
    {
        _api = api;

        Text = "Weekoverzicht omzet";
        Width = 1000;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;

        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
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

        FlowLayoutPanel kpis = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        _totaal = CreateTile(kpis, "Totaal omzet");
        _gemiddeld = CreateTile(kpis, "Gemiddeld per week");
        _besteWeek = CreateTile(kpis, "Beste week");
        _topGerecht = CreateTile(kpis, "Topgerecht");

        TabControl tabs = new TabControl { Dock = DockStyle.Fill };
        AddTab(tabs, "Per week", _perWeek);
        AddTab(tabs, "Per gerecht", _perGerecht);
        AddTab(tabs, "Gestapeld per week", _gestapeld);
        AddTab(tabs, "Tabel", _grid);

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(kpis, 0, 1);
        layout.Controls.Add(tabs, 0, 2);
        layout.Controls.Add(_melding, 0, 3);

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

        // De grafieken tonen de weken van oud naar nieuw; de API levert ze van nieuw naar oud.
        List<WeekTotaal> weken = regels
            .GroupBy(regel => (regel.Jaar, regel.Week))
            .OrderBy(groep => groep.Key.Jaar).ThenBy(groep => groep.Key.Week)
            .Select(groep => new WeekTotaal($"{groep.Key.Jaar}-W{groep.Key.Week:00}", groep.ToList()))
            .ToList();
        List<string> gerechten = regels
            .GroupBy(regel => regel.Gerecht)
            .OrderByDescending(groep => groep.Sum(regel => regel.Omzet))
            .Select(groep => groep.Key)
            .ToList();

        UpdateKpis(regels, weken);
        DrawPerWeek(weken);
        DrawPerGerecht(regels, gerechten);
        DrawGestapeld(weken, gerechten);

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

    private void UpdateKpis(List<WeekOmzetRegel> regels, List<WeekTotaal> weken)
    {
        if (regels.Count == 0)
        {
            _totaal.Text = "–";
            _gemiddeld.Text = "–";
            _besteWeek.Text = "–";
            _topGerecht.Text = "–";
            return;
        }

        decimal totaal = regels.Sum(regel => regel.Omzet);
        WeekTotaal beste = weken.MaxBy(week => week.Omzet)!;
        var top = regels
            .GroupBy(regel => regel.Gerecht)
            .Select(groep => new { Gerecht = groep.Key, Omzet = groep.Sum(regel => regel.Omzet) })
            .MaxBy(gerecht => gerecht.Omzet)!;

        _totaal.Text = "€ " + totaal.ToString("N2", DutchCulture);
        _gemiddeld.Text = "€ " + (totaal / weken.Count).ToString("N2", DutchCulture);
        _besteWeek.Text = $"{beste.Label}  (€ {beste.Omzet.ToString("N0", DutchCulture)})";
        _topGerecht.Text = $"{top.Gerecht}  (€ {top.Omzet.ToString("N0", DutchCulture)})";
    }

    private void DrawPerWeek(List<WeekTotaal> weken)
    {
        ScottPlot.Plot plot = _perWeek.Plot;
        plot.Clear();

        if (weken.Count > 0)
        {
            plot.Add.Bars(weken.Select(week => (double)week.Omzet).ToArray());
            SetWeekTicks(plot, weken);
        }

        plot.Axes.Left.TickGenerator = CreateEuroTicks();
        plot.Axes.Margins(bottom: 0);
        plot.Axes.AutoScale();
        _perWeek.Refresh();
    }

    private void DrawPerGerecht(List<WeekOmzetRegel> regels, List<string> gerechten)
    {
        ScottPlot.Plot plot = _perGerecht.Plot;
        plot.Clear();

        if (gerechten.Count > 0)
        {
            // Hoogste omzet bovenaan: positie 0 ligt onderaan, dus de lijst wordt omgedraaid.
            List<string> volgorde = gerechten.AsEnumerable().Reverse().ToList();
            double[] omzet = volgorde
                .Select(gerecht => (double)regels.Where(regel => regel.Gerecht == gerecht).Sum(regel => regel.Omzet))
                .ToArray();

            ScottPlot.Plottables.BarPlot bars = plot.Add.Bars(omzet);
            bars.Horizontal = true;
            plot.Axes.Left.SetTicks(Enumerable.Range(0, volgorde.Count).Select(i => (double)i).ToArray(), volgorde.ToArray());
        }

        plot.Axes.Bottom.TickGenerator = CreateEuroTicks();
        plot.Axes.Margins(left: 0);
        plot.Axes.AutoScale();
        _perGerecht.Refresh();
    }

    private void DrawGestapeld(List<WeekTotaal> weken, List<string> gerechten)
    {
        ScottPlot.Plot plot = _gestapeld.Plot;
        plot.Clear();
        plot.Legend.ManualItems.Clear();

        if (weken.Count > 0)
        {
            ScottPlot.Palettes.Category10 palette = new ScottPlot.Palettes.Category10();
            List<ScottPlot.Bar> bars = new List<ScottPlot.Bar>();

            for (int i = 0; i < weken.Count; i++)
            {
                double basis = 0;
                for (int g = 0; g < gerechten.Count; g++)
                {
                    double omzet = (double)weken[i].Regels.Where(regel => regel.Gerecht == gerechten[g]).Sum(regel => regel.Omzet);
                    if (omzet == 0)
                    {
                        continue;
                    }

                    bars.Add(new ScottPlot.Bar
                    {
                        Position = i,
                        ValueBase = basis,
                        Value = basis + omzet,
                        FillColor = palette.GetColor(g)
                    });
                    basis += omzet;
                }
            }

            plot.Add.Bars(bars);
            for (int g = 0; g < gerechten.Count; g++)
            {
                plot.Legend.ManualItems.Add(new ScottPlot.LegendItem { LabelText = gerechten[g], FillColor = palette.GetColor(g) });
            }

            plot.ShowLegend(ScottPlot.Alignment.UpperLeft);
            SetWeekTicks(plot, weken);
        }
        else
        {
            plot.HideLegend();
        }

        plot.Axes.Left.TickGenerator = CreateEuroTicks();
        plot.Axes.Margins(bottom: 0);
        plot.Axes.AutoScale();
        _gestapeld.Refresh();
    }

    private static void SetWeekTicks(ScottPlot.Plot plot, List<WeekTotaal> weken)
    {
        double[] posities = Enumerable.Range(0, weken.Count).Select(i => (double)i).ToArray();
        plot.Axes.Bottom.SetTicks(posities, weken.Select(week => week.Label).ToArray());

        // Bij veel weken passen de labels niet naast elkaar, dus die worden schuin gezet.
        bool schuin = weken.Count > 12;
        plot.Axes.Bottom.TickLabelStyle.Rotation = schuin ? 45 : 0;
        plot.Axes.Bottom.TickLabelStyle.Alignment = schuin ? ScottPlot.Alignment.MiddleLeft : ScottPlot.Alignment.UpperCenter;
    }

    private static ScottPlot.TickGenerators.NumericAutomatic CreateEuroTicks()
    {
        return new ScottPlot.TickGenerators.NumericAutomatic
        {
            LabelFormatter = value => "€ " + value.ToString("N0", DutchCulture)
        };
    }

    private static FormsPlot CreatePlot()
    {
        return new FormsPlot { Dock = DockStyle.Fill };
    }

    private static Label CreateTile(FlowLayoutPanel parent, string titel)
    {
        Panel tile = new Panel { Width = 230, Height = 64, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 0, 8, 0) };
        Label waarde = new Label
        {
            Text = "–",
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            Padding = new Padding(6, 0, 6, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(SystemFonts.DefaultFont.FontFamily, 13, FontStyle.Bold)
        };
        Label kop = new Label { Text = titel, Dock = DockStyle.Top, Height = 20, ForeColor = SystemColors.GrayText, Padding = new Padding(6, 4, 6, 0) };
        tile.Controls.Add(waarde);
        tile.Controls.Add(kop);
        parent.Controls.Add(tile);
        return waarde;
    }

    private static void AddTab(TabControl tabs, string titel, Control inhoud)
    {
        TabPage page = new TabPage(titel);
        page.Controls.Add(inhoud);
        tabs.TabPages.Add(page);
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

    private sealed class WeekTotaal
    {
        public WeekTotaal(string label, List<WeekOmzetRegel> regels)
        {
            Label = label;
            Regels = regels;
            Omzet = regels.Sum(regel => regel.Omzet);
        }

        public string Label { get; }
        public List<WeekOmzetRegel> Regels { get; }
        public decimal Omzet { get; }
    }

    private sealed class WeekOmzetRow
    {
        public string Week { get; set; } = "";
        public string Hoofdgroep { get; set; } = "";
        public string Omzet { get; set; } = "";
    }
}
