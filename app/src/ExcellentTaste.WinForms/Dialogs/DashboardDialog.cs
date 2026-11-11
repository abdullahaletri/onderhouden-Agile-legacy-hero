using System.Drawing.Drawing2D;
using System.Globalization;
using ExcellentTaste.Core.Models;
using ExcellentTaste.WinForms.Api;

namespace ExcellentTaste.WinForms.Dialogs;

internal sealed class DashboardDialog : Form
{
    private static readonly CultureInfo Nl = new CultureInfo("nl-NL");

    private readonly ExcellentTasteApiClient _api;

    private readonly Label _omzetVandaag = MaakWaardeLabel();
    private readonly Label _omzetDezeWeek = MaakWaardeLabel();
    private readonly Label _aantalBestellingen = MaakWaardeLabel();

    private readonly TaartGrafiek _betaalwijzeGrafiek = new TaartGrafiek { Titel = "Omzet per betaalwijze" };
    private readonly StaafGrafiek _hoofdgroepGrafiek = new StaafGrafiek { Titel = "Omzet per hoofdgroep (deze week)" };
    private readonly LijnGrafiek _weekGrafiek = new LijnGrafiek { Titel = "Omzet per week (laatste 8 weken)" };

    public DashboardDialog(ExcellentTasteApiClient api)
    {
        _api = api;

        Text = "Dashboard";
        Width = 1050;
        Height = 760;
        MinimumSize = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;

        TableLayoutPanel grafieken = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2
        };
        grafieken.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grafieken.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grafieken.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        grafieken.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        _betaalwijzeGrafiek.Dock = DockStyle.Fill;
        _hoofdgroepGrafiek.Dock = DockStyle.Fill;
        _weekGrafiek.Dock = DockStyle.Fill;

        grafieken.Controls.Add(_betaalwijzeGrafiek, 0, 0);
        grafieken.Controls.Add(_hoofdgroepGrafiek, 1, 0);
        grafieken.Controls.Add(_weekGrafiek, 0, 1);
        grafieken.SetColumnSpan(_weekGrafiek, 2);

        Panel boven = new Panel { Dock = DockStyle.Top, Height = 130 };

        TableLayoutPanel tabel = new TableLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 460,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(24, 12, 0, 0)
        };
        tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        VoegRijToe(tabel, 0, "Omzet vandaag", _omzetVandaag);
        VoegRijToe(tabel, 1, "Omzet deze week", _omzetDezeWeek);
        VoegRijToe(tabel, 2, "Bestellingen vandaag", _aantalBestellingen);

        Button vernieuwen = new Button { Text = "Vernieuwen", Width = 112, Height = 32, Location = new Point(500, 24) };
        Button sluiten = new Button { Text = "Sluiten", Width = 112, Height = 32, Location = new Point(500, 66) };
        vernieuwen.Click += delegate { Vernieuwen(); };
        sluiten.Click += delegate { Close(); };

        boven.Controls.Add(vernieuwen);
        boven.Controls.Add(sluiten);
        boven.Controls.Add(tabel);

        Controls.Add(grafieken);
        Controls.Add(boven);
    }

    // Wordt aangeroepen door Form1 voordat de dialoog opent.
    public void Laden()
    {
        DashboardData data = _api.GetDashboard();

        _omzetVandaag.Text = data.OmzetVandaag.ToString("C", Nl);
        _omzetDezeWeek.Text = data.OmzetDezeWeek.ToString("C", Nl);
        _aantalBestellingen.Text = data.AantalBestellingenVandaag.ToString(Nl);

        _betaalwijzeGrafiek.SetData(data.OmzetPerBetaalwijze.Select(x => (x.Betaalwijze, x.Omzet)));
        _hoofdgroepGrafiek.SetData(data.OmzetPerHoofdgroep.Select(x => (x.Hoofdgroep, x.Omzet)));
        _weekGrafiek.SetData(data.OmzetPerWeek.Select(x => ("wk " + x.Weeknummer, x.Omzet)));
    }

    private void Vernieuwen()
    {
        try
        {
            Laden();
        }
        catch (ApiUnavailableException)
        {
            MessageBox.Show(this, "De API is niet bereikbaar.\r\n\r\nNeem contact op met de administrator.",
                "API niet bereikbaar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void VoegRijToe(TableLayoutPanel tabel, int rij, string titel, Label waarde)
    {
        Label label = new Label { Text = titel, AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 11F) };
        tabel.Controls.Add(label, 0, rij);
        tabel.Controls.Add(waarde, 1, rij);
    }

    private static Label MaakWaardeLabel()
    {
        return new Label
        {
            Text = "-",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };
    }
}

// ---------------------------------------------------------------
// Zelfgetekende grafieken (GDI+), zonder externe pakketten.
// ---------------------------------------------------------------

internal abstract class GrafiekBasis : Control
{
    protected static readonly CultureInfo Nl = new CultureInfo("nl-NL");

    protected static readonly Color[] Kleuren =
    {
        Color.FromArgb(66, 133, 244),
        Color.FromArgb(234, 67, 53),
        Color.FromArgb(251, 188, 5),
        Color.FromArgb(52, 168, 83),
        Color.FromArgb(171, 71, 188),
        Color.FromArgb(255, 112, 67),
        Color.FromArgb(0, 172, 193),
        Color.FromArgb(158, 158, 158)
    };

    private List<(string Label, decimal Waarde)> _data = new List<(string Label, decimal Waarde)>();

    public string Titel { get; set; } = "";

    protected List<(string Label, decimal Waarde)> Data => _data;

    protected GrafiekBasis()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
    }

    public void SetData(IEnumerable<(string Label, decimal Waarde)> data)
    {
        _data = data.ToList();
        Invalidate();
    }

    private bool IsLeeg()
    {
        if (_data.Count == 0)
        {
            return true;
        }

        foreach (var item in _data)
        {
            if (item.Waarde > 0)
            {
                return false;
            }
        }

        return true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        try
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            using (Font titelFont = new Font("Segoe UI", 11F, FontStyle.Bold))
            {
                g.DrawString(Titel, titelFont, Brushes.Black, 8, 6);
            }

            Rectangle gebied = new Rectangle(12, 40, Width - 24, Height - 52);
            if (gebied.Width < 60 || gebied.Height < 60)
            {
                return;
            }

            if (IsLeeg())
            {
                using (Font font = new Font("Segoe UI", 10F, FontStyle.Italic))
                using (StringFormat midden = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString("Nog geen omzet", font, Brushes.Gray, gebied, midden);
                }
                return;
            }

            Teken(g, gebied);
        }
        catch (Exception)
        {
            // Een tekenfout mag het scherm nooit laten crashen.
        }
    }

    protected abstract void Teken(Graphics g, Rectangle gebied);

    protected static decimal Max(List<(string Label, decimal Waarde)> data)
    {
        decimal max = 0;
        foreach (var item in data)
        {
            if (item.Waarde > max)
            {
                max = item.Waarde;
            }
        }

        return max <= 0 ? 1 : max;
    }
}

internal sealed class TaartGrafiek : GrafiekBasis
{
    protected override void Teken(Graphics g, Rectangle gebied)
    {
        decimal totaal = 0;
        foreach (var item in Data)
        {
            if (item.Waarde > 0)
            {
                totaal += item.Waarde;
            }
        }

        int grootte = Math.Min(gebied.Height, gebied.Width / 2);
        Rectangle taart = new Rectangle(gebied.X, gebied.Y + (gebied.Height - grootte) / 2, grootte, grootte);

        float start = -90f;
        int kleurIndex = 0;
        foreach (var item in Data)
        {
            if (item.Waarde <= 0)
            {
                continue;
            }

            float hoek = (float)(item.Waarde / totaal * 360m);
            using (SolidBrush kwast = new SolidBrush(Kleuren[kleurIndex % Kleuren.Length]))
            {
                g.FillPie(kwast, taart, start, hoek);
            }

            start += hoek;
            kleurIndex++;
        }

        // Legenda rechts van de taart
        int x = taart.Right + 24;
        int y = gebied.Y + 10;
        kleurIndex = 0;
        using (Font font = new Font("Segoe UI", 9.5F))
        {
            foreach (var item in Data)
            {
                if (item.Waarde <= 0)
                {
                    continue;
                }

                using (SolidBrush kwast = new SolidBrush(Kleuren[kleurIndex % Kleuren.Length]))
                {
                    g.FillRectangle(kwast, x, y + 2, 12, 12);
                }

                g.DrawString(item.Label + ": " + item.Waarde.ToString("C2", Nl), font, Brushes.Black, x + 18, y);
                y += 24;
                kleurIndex++;
            }
        }
    }
}

internal sealed class StaafGrafiek : GrafiekBasis
{
    protected override void Teken(Graphics g, Rectangle gebied)
    {
        int labelHoogte = 36;
        int bovenRuimte = 20;
        Rectangle vlak = new Rectangle(gebied.X, gebied.Y + bovenRuimte, gebied.Width, gebied.Height - bovenRuimte - labelHoogte);
        decimal max = Max(Data);

        float slot = (float)vlak.Width / Data.Count;
        float balkBreedte = slot * 0.6f;

        using (Font font = new Font("Segoe UI", 8.5F))
        using (StringFormat midden = new StringFormat { Alignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter })
        using (Pen as_ = new Pen(Color.Gray))
        {
            g.DrawLine(as_, vlak.Left, vlak.Bottom, vlak.Right, vlak.Bottom);

            for (int i = 0; i < Data.Count; i++)
            {
                var item = Data[i];
                float hoogte = (float)(item.Waarde / max) * vlak.Height;
                float x = vlak.X + slot * i + (slot - balkBreedte) / 2;
                float y = vlak.Bottom - hoogte;

                using (SolidBrush kwast = new SolidBrush(Kleuren[i % Kleuren.Length]))
                {
                    g.FillRectangle(kwast, x, y, balkBreedte, hoogte);
                }

                RectangleF waardeVak = new RectangleF(vlak.X + slot * i, y - 18, slot, 16);
                g.DrawString(item.Waarde.ToString("N2", Nl), font, Brushes.Black, waardeVak, midden);

                RectangleF labelVak = new RectangleF(vlak.X + slot * i, vlak.Bottom + 4, slot, labelHoogte - 4);
                g.DrawString(item.Label, font, Brushes.Black, labelVak, midden);
            }
        }
    }
}

internal sealed class LijnGrafiek : GrafiekBasis
{
    protected override void Teken(Graphics g, Rectangle gebied)
    {
        int labelHoogte = 22;
        int bovenRuimte = 22;
        Rectangle vlak = new Rectangle(gebied.X + 10, gebied.Y + bovenRuimte, gebied.Width - 20, gebied.Height - bovenRuimte - labelHoogte);
        decimal max = Max(Data);

        float slot = (float)vlak.Width / Data.Count;
        PointF[] punten = new PointF[Data.Count];

        for (int i = 0; i < Data.Count; i++)
        {
            float x = vlak.X + slot * i + slot / 2;
            float y = vlak.Bottom - (float)(Data[i].Waarde / max) * vlak.Height;
            punten[i] = new PointF(x, y);
        }

        using (Font font = new Font("Segoe UI", 8.5F))
        using (StringFormat midden = new StringFormat { Alignment = StringAlignment.Center })
        using (Pen as_ = new Pen(Color.Gray))
        using (Pen lijn = new Pen(Kleuren[0], 3f))
        using (SolidBrush punt = new SolidBrush(Kleuren[0]))
        {
            g.DrawLine(as_, vlak.Left, vlak.Bottom, vlak.Right, vlak.Bottom);

            if (punten.Length >= 2)
            {
                g.DrawLines(lijn, punten);
            }

            for (int i = 0; i < punten.Length; i++)
            {
                g.FillEllipse(punt, punten[i].X - 5, punten[i].Y - 5, 10, 10);

                RectangleF waardeVak = new RectangleF(punten[i].X - slot / 2, punten[i].Y - 22, slot, 16);
                g.DrawString(Data[i].Waarde.ToString("N0", Nl), font, Brushes.Black, waardeVak, midden);

                RectangleF labelVak = new RectangleF(vlak.X + slot * i, vlak.Bottom + 4, slot, labelHoogte - 4);
                g.DrawString(Data[i].Label, font, Brushes.Black, labelVak, midden);
            }
        }
    }
}