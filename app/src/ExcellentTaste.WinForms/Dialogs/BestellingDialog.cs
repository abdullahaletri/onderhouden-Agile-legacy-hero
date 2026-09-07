using System.ComponentModel;
using System.Drawing.Printing;
using ExcellentTaste.Core.Models;
using ExcellentTaste.WinForms.Api;
using static ExcellentTaste.WinForms.AppCulture;
using static ExcellentTaste.WinForms.WinFormsUi;

namespace ExcellentTaste.WinForms.Dialogs;

internal sealed class BestellingDialog : Form
{
    private readonly ExcellentTasteApiClient _api;
    private readonly Reservering _reservering;
    private readonly DataGridView _grid = CreateGrid();
    private readonly TabControl _menuTabs = new TabControl { Dock = DockStyle.Fill };
    private readonly Label _totaal = new Label { AutoSize = true, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) };

    public BestellingDialog(ExcellentTasteApiClient api, Reservering reservering)
    {
        _api = api;
        _reservering = reservering;

        Text = $"Bestelling voor tafel {reservering.Tafel}";
        Width = 1200;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;

        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
        Controls.Add(layout);

        TableLayoutPanel left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 4
        };
        left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        Label titel = new Label
        {
            Text = $"Bestelling voor tafel {reservering.Tafel}",
            Dock = DockStyle.Fill,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
        };
        FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, FlowDirection = FlowDirection.LeftToRight };
        Button plus = CreateButton("+", 42);
        Button min = CreateButton("-", 42);
        Button delete = CreateButton("Verwijderen", 112);
        Button print = CreateButton("Print bon", 96);
        Button close = CreateButton("Terug");
        toolbar.Controls.AddRange(new Control[] { plus, min, delete, print, close });
        toolbar.Dock = DockStyle.Fill;
        _totaal.Dock = DockStyle.Fill;
        _grid.Dock = DockStyle.Fill;
        left.Controls.Add(titel, 0, 0);
        left.Controls.Add(toolbar, 0, 1);
        left.Controls.Add(_grid, 0, 2);
        left.Controls.Add(_totaal, 0, 3);

        TableLayoutPanel right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 2
        };
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Label menuTitel = new Label
        {
            Text = "Menu",
            Dock = DockStyle.Fill,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold)
        };
        right.Controls.Add(menuTitel, 0, 0);
        right.Controls.Add(_menuTabs, 0, 1);

        layout.Controls.Add(left, 0, 0);
        layout.Controls.Add(right, 1, 0);

        plus.Click += delegate { TryRunApiAction(() => ChangeSelected(1)); };
        min.Click += delegate { TryRunApiAction(() => ChangeSelected(-1)); };
        delete.Click += delegate { TryRunApiAction(DeleteSelected); };
        print.Click += delegate { TryRunApiAction(PrintBon); };
        close.Click += delegate { Close(); };

        RenderMenu();
        LoadBestelling();
    }

    private void RenderMenu()
    {
        _menuTabs.TabPages.Clear();
        List<Gerecht> gerechten = _api.GetGerechten();
        List<Subgerecht> subgerechten = _api.GetSubgerechten();
        List<MenuItem> menuItems = _api.GetMenuItems();

        foreach (Gerecht gerecht in gerechten)
        {
            TabPage tab = new TabPage(gerecht.GerechtNaam);
            FlowLayoutPanel page = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(8)
            };

            foreach (Subgerecht subgerecht in subgerechten)
            {
                if (subgerecht.GerechtCode != gerecht.GerechtCode)
                {
                    continue;
                }

                GroupBox group = new GroupBox
                {
                    Text = subgerecht.SubgerechtNaam,
                    Width = 320,
                    Height = 52,
                    Padding = new Padding(8),
                    Margin = new Padding(6)
                };
                FlowLayoutPanel items = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false
                };
                group.Controls.Add(items);

                foreach (MenuItem item in menuItems)
                {
                    if (item.SubgerechtCode != subgerecht.SubgerechtCode)
                    {
                        continue;
                    }

                    Button button = new Button
                    {
                        Text = item.MenuItemNaam,
                        Tag = item.MenuItemCode,
                        Width = 285,
                        Height = 34,
                        TextAlign = ContentAlignment.MiddleLeft
                    };
                    button.Click += delegate
                    {
                        TryRunApiAction(() =>
                        {
                            _api.AddBestellingItem(_reservering.ReserveringId, (string)button.Tag);
                            LoadBestelling();
                        });
                    };
                    items.Controls.Add(button);
                }

                if (items.Controls.Count > 0)
                {
                    group.Height = 28 + (items.Controls.Count * 40);
                    page.Controls.Add(group);
                }
            }

            tab.Controls.Add(page);
            page.Resize += delegate { ResizeMenuGroups(page); };
            ResizeMenuGroups(page);
            _menuTabs.TabPages.Add(tab);
        }
    }

    private static void ResizeMenuGroups(FlowLayoutPanel page)
    {
        int width = Math.Max(260, page.ClientSize.Width - 28);
        foreach (Control control in page.Controls)
        {
            GroupBox? group = control as GroupBox;
            if (group == null)
            {
                continue;
            }

            group.Width = width;
            foreach (Control groupControl in group.Controls)
            {
                FlowLayoutPanel? panel = groupControl as FlowLayoutPanel;
                if (panel == null)
                {
                    continue;
                }

                foreach (Control buttonControl in panel.Controls)
                {
                    Button? button = buttonControl as Button;
                    if (button != null)
                    {
                        button.Width = Math.Max(220, width - 36);
                    }
                }
            }
        }
    }

    private void LoadBestelling()
    {
        List<Bestelling> bestellingen = _api.GetBestellingen(_reservering.ReserveringId);
        List<BestellingRow> rows = new List<BestellingRow>();

        foreach (Bestelling bestelling in bestellingen)
        {
            BestellingRow row = new BestellingRow();
            row.MenuItemCode = bestelling.MenuItemCode;
            row.Naam = bestelling.MenuItemNaam;
            row.Aantal = bestelling.Aantal;
            row.Prijs = bestelling.Prijs.ToString("0.00", DutchCulture);
            rows.Add(row);
        }

        _grid.DataSource = new BindingList<BestellingRow>(rows);
        HideColumn(_grid, nameof(BestellingRow.MenuItemCode));

        decimal totaal = BerekenTotaal();
        _totaal.Text = "Totaal: " + totaal.ToString("0.00", DutchCulture);
    }

    private decimal BerekenTotaal()
    {
        decimal totaal = 0;
        List<BonRegel> bonRegels = _api.GetBon(_reservering.ReserveringId);

        foreach (BonRegel regel in bonRegels)
        {
            totaal += regel.Totaal;
        }

        return totaal;
    }

    private void ChangeSelected(int stap)
    {
        string? code = GetSelectedCode();
        if (code == null)
        {
            return;
        }

        if (stap > 0)
        {
            _api.PlusBestellingItem(_reservering.ReserveringId, code);
        }
        else
        {
            _api.MinBestellingItem(_reservering.ReserveringId, code);
        }

        LoadBestelling();
    }

    private void DeleteSelected()
    {
        string? code = GetSelectedCode();
        if (code == null)
        {
            return;
        }

        _api.DeleteBestellingItem(_reservering.ReserveringId, code);
        LoadBestelling();
    }

    private void PrintBon()
    {
        List<BonRegel> bonRegels = _api.GetBon(_reservering.ReserveringId);

        using PrintDocument document = new PrintDocument();
        document.DocumentName = $"Bon tafel {_reservering.Tafel}";
        document.PrintPage += delegate(object sender, PrintPageEventArgs e)
        {
            if (e.Graphics != null)
            {
                PrintBonPage(e.Graphics, bonRegels);
            }
        };

        using PrintDialog dialog = new PrintDialog
        {
            Document = document,
            UseEXDialog = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            document.Print();
        }
    }

    private void PrintBonPage(Graphics graphics, List<BonRegel> bonRegels)
    {
        using Font titleFont = new Font("Arial", 14, FontStyle.Bold);
        using Font normalFont = new Font("Arial", 10);
        using Font boldFont = new Font("Arial", 10, FontStyle.Bold);

        float x = 60;
        float y = 60;
        decimal totaal = 0;

        graphics.DrawString("Restaurant Excellent Taste", titleFont, Brushes.Black, x, y);
        y += 32;
        graphics.DrawString($"Bon voor tafel {_reservering.Tafel}", boldFont, Brushes.Black, x, y);
        y += 28;

        foreach (BonRegel regel in bonRegels)
        {
            string regelTekst = $"{regel.Aantal} x {regel.MenuItemNaam}  {regel.Totaal.ToString("0.00", DutchCulture)}";
            graphics.DrawString(regelTekst, normalFont, Brushes.Black, x, y);
            y += 22;
            totaal += regel.Totaal;
        }

        y += 12;
        graphics.DrawString("Totaal: " + totaal.ToString("0.00", DutchCulture), boldFont, Brushes.Black, x, y);
    }

    private string? GetSelectedCode()
    {
        if (_grid.CurrentRow == null)
        {
            return null;
        }

        BestellingRow? row = _grid.CurrentRow.DataBoundItem as BestellingRow;
        if (row == null)
        {
            return null;
        }

        return row.MenuItemCode;
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
}
