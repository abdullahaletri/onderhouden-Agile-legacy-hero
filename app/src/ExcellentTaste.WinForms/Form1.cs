using System.ComponentModel;
using System.Globalization;
using ExcellentTaste.Core.Models;
using ExcellentTaste.WinForms.Api;
using ExcellentTaste.WinForms.Controls;
using ExcellentTaste.WinForms.Dialogs;
using static ExcellentTaste.WinForms.AppConstants;
using static ExcellentTaste.WinForms.AppCulture;
using static ExcellentTaste.WinForms.WinFormsUi;

namespace ExcellentTaste.WinForms;

public class Form1 : Form
{
    private const string ApiUnavailableMessage = "De API is niet bereikbaar.\r\n\r\nNeem contact op met de administrator.";

    private readonly ExcellentTasteApiClient _api;

    private readonly TabControl _tabs = new TabControl { Dock = DockStyle.Fill };
    private readonly Label _status = new Label { Dock = DockStyle.Bottom, Height = 28, Padding = new Padding(8, 6, 8, 0) };

    private readonly DataGridView _reserveringenGrid = CreateGrid();
    private readonly Button _reserveringWijzigen = CreateButton("Wijzigen");
    private readonly Button _reserveringVerwijderen = CreateButton("Verwijderen", 112);
    private readonly Button _reserveringBestelling = CreateButton("Bestelling");

    private readonly ComboBox _overzichtSoort = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _overzichtGrid = CreateGrid();

    private readonly ComboBox _gegevensSoort = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _gegevensGrid = CreateGrid();
    private readonly Button _gegevensNieuw = CreateButton("Nieuw");
    private readonly Button _gegevensWijzigen = CreateButton("Wijzigen");
    private readonly Button _gegevensVerwijderen = CreateButton("Verwijderen", 112);

    public Form1()
    {
        Text = "Excellent Taste";
        Width = 1200;
        Height = 760;
        MinimumSize = new Size(980, 620);
        StartPosition = FormStartPosition.CenterScreen;

        _api = new ExcellentTasteApiClient(ApiBaseUrl);

        Controls.Add(_tabs);
        Controls.Add(_status);

        BuildHomeTab();
        BuildReserveringenTab();
        BuildOverzichtenTab();
        BuildGegevensTab();
        _tabs.SelectedIndexChanged += delegate { TryRunApiAction(LoadActiveTab); };

        if (TryInitializeApi())
        {
            TryRunApiAction(LoadReserveringen);
            TryRunApiAction(LoadOverzicht);
            TryRunApiAction(LoadGegevens);
            SetStatus("Klaar.");
        }
        else
        {
            SetStatus("API niet bereikbaar.");
        }
    }

    private void BuildHomeTab()
    {
        TableLayoutPanel page = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(24)
        };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));

        Label text = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = false,
            Height = 180,
            Text = "Welkom bij de reserverings- en bestellingenapplicatie van Restaurant Excellent Taste.\r\n\r\n" +
                   "Vul eerst een reservering in. Deze kan telefonisch binnenkomen of kan worden ingevoerd als gasten plaatsnemen aan een vrije tafel.\r\n\r\n" +
                   "Daarna kan een bestelling worden opgenomen.",
        };
        page.Controls.Add(text, 0, 0);

        PictureBox image = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom };
        string? imagePath = FindRestaurantImage();
        if (imagePath != null)
        {
            image.ImageLocation = imagePath;
        }
        page.Controls.Add(image, 1, 0);

        _tabs.TabPages.Add(new TabPage("Home") { Controls = { page } });
    }

    private void BuildReserveringenTab()
    {
        ToolbarGridPage page = new ToolbarGridPage();
        Button nieuw = CreateButton("Nieuw");
        Button refresh = CreateButton("Vernieuwen", 112);
        page.Toolbar.Controls.AddRange(new Control[] { nieuw, _reserveringWijzigen, _reserveringVerwijderen, _reserveringBestelling, refresh });

        page.SetContent(_reserveringenGrid);

        nieuw.Click += delegate { TryRunApiAction(() => EditReservering(null)); };
        _reserveringWijzigen.Click += delegate { TryRunApiAction(() => EditReservering(GetSelectedReserveringId())); };
        _reserveringVerwijderen.Click += delegate { TryRunApiAction(DeleteReservering); };
        _reserveringBestelling.Click += delegate { TryRunApiAction(OpenBestellingFromSelection); };
        refresh.Click += delegate { TryRunApiAction(LoadReserveringen); };
        _reserveringenGrid.CellDoubleClick += delegate { TryRunApiAction(OpenBestellingFromSelection); };
        _reserveringenGrid.RowPrePaint += delegate (object? sender, DataGridViewRowPrePaintEventArgs e) { ColorReserveringRow(e.RowIndex); };

        _tabs.TabPages.Add(new TabPage("Reserveringen") { Controls = { page } });
    }

    private void BuildOverzichtenTab()
    {
        ToolbarGridPage page = new ToolbarGridPage();
        _overzichtSoort.Width = 180;
        _overzichtSoort.DisplayMember = nameof(SoortItem.Label);
        _overzichtSoort.ValueMember = nameof(SoortItem.Value);
        _overzichtSoort.Items.AddRange(new object[]
        {
            new SoortItem("kok", "Kok"),
            new SoortItem("ober", "Ober")
        });
        _overzichtSoort.SelectedIndex = 0;
        Button refresh = CreateButton("Vernieuwen", 112);
        page.Toolbar.Controls.AddRange(new Control[] { _overzichtSoort, refresh });

        page.SetContent(_overzichtGrid);

        _overzichtSoort.SelectedIndexChanged += delegate { TryRunApiAction(LoadOverzicht); };
        refresh.Click += delegate { TryRunApiAction(LoadOverzicht); };

        _tabs.TabPages.Add(new TabPage("Overzichten") { Controls = { page } });
    }

    private void BuildGegevensTab()
    {
        ToolbarGridPage page = new ToolbarGridPage();
        _gegevensSoort.Width = 180;
        _gegevensSoort.DisplayMember = nameof(SoortItem.Label);
        _gegevensSoort.ValueMember = nameof(SoortItem.Value);
        _gegevensSoort.Items.AddRange(new object[]
        {
            new SoortItem("drinken", "Drinken"),
            new SoortItem("eten", "Eten"),
            new SoortItem("klanten", "Klanten"),
            new SoortItem("gerechten", "Gerecht hoofdgroepen"),
            new SoortItem("subgerechten", "Gerecht subgroepen")
        });
        _gegevensSoort.SelectedIndex = 0;
        Button refresh = CreateButton("Vernieuwen", 112);
        page.Toolbar.Controls.AddRange(new Control[] { _gegevensSoort, _gegevensNieuw, _gegevensWijzigen, _gegevensVerwijderen, refresh });

        page.SetContent(_gegevensGrid);

        _gegevensSoort.SelectedIndexChanged += delegate { TryRunApiAction(LoadGegevens); };
        _gegevensNieuw.Click += delegate { TryRunApiAction(() => EditGegevens(null)); };
        _gegevensWijzigen.Click += delegate { TryRunApiAction(() => EditGegevens(GetSelectedGegevensKey())); };
        _gegevensVerwijderen.Click += delegate { TryRunApiAction(DeleteGegevens); };
        refresh.Click += delegate { TryRunApiAction(LoadGegevens); };
        _gegevensGrid.CellDoubleClick += delegate { TryRunApiAction(() => EditGegevens(GetSelectedGegevensKey())); };

        _tabs.TabPages.Add(new TabPage("Gegevens") { Controls = { page } });
    }

    private void LoadActiveTab()
    {
        string? tabName = _tabs.SelectedTab?.Text;

        switch (tabName)
        {
            case "Reserveringen":
                LoadReserveringen();
                break;
            case "Overzichten":
                LoadOverzicht();
                break;
            case "Gegevens":
                LoadGegevens();
                break;
        }
    }

    private void LoadReserveringen()
    {
        List<ReserveringRow> rows = new List<ReserveringRow>();
        List<Reservering> reserveringen = _api.GetReserveringen();

        foreach (Reservering reservering in reserveringen)
        {
            ReserveringRow row = new ReserveringRow();
            row.Id = reservering.ReserveringId;
            row.DatumWaarde = reservering.Datum;
            row.Datum = reservering.Datum.ToString("dd-MM-yyyy", DutchCulture);
            row.Tijd = reservering.Tijd.ToString("HH:mm", DutchCulture);
            row.Tafel = reservering.Tafel;
            row.Klant = reservering.KlantNaam;
            row.Telefoon = reservering.Telefoon;
            row.Aantal = reservering.Aantal;
            row.Kinderen = reservering.AantalKinderen;
            row.Opmerkingen = "";


            if (reservering.Opmerkingen != null)
            {
                row.Opmerkingen = reservering.Opmerkingen;
            }

            rows.Add(row);
        }

        _reserveringenGrid.DataSource = new BindingList<ReserveringRow>(rows);
        HideColumn(_reserveringenGrid, nameof(ReserveringRow.Id));
        HideColumn(_reserveringenGrid, nameof(ReserveringRow.DatumWaarde));
        SetStatus("Reserveringen geladen.");
    }

    private void EditReservering(int? reserveringId)
    {
        Reservering? reservering;

        if (reserveringId == null)
        {
            reservering = new Reservering();
            reservering.Datum = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            reservering.Tijd = new TimeOnly(18, 0);
            reservering.Aantal = 2;
            reservering.Status = 1;
        }
        else
        {
            reservering = _api.GetReservering(reserveringId.Value);
        }

        if (reservering == null)
        {
            SetStatus("Onbekende reservering.");
            return;
        }

        using ReserveringDialog dialog = new ReserveringDialog(_api.GetKlanten(), reservering);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        int klantId = dialog.SelectedKlantId;
        if (klantId == NieuweKlantId)
        {
            Klant nieuweKlant = dialog.NieuweKlant;
            if (string.IsNullOrWhiteSpace(nieuweKlant.KlantNaam))
            {
                MessageBox.Show(this, "Vul een klantnaam in voor een nieuwe klant.", "Reservering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            klantId = _api.AddKlant(nieuweKlant);
        }

        Reservering updated = dialog.ToReservering(klantId);
        ApiResult result;

        if (reserveringId == null)
        {
            result = _api.AddReservering(updated);
        }
        else
        {
            result = UpdateReservering(reserveringId.Value, updated);
        }

        if (!result.Gelukt)
        {
            MessageBox.Show(this, result.Bericht, "Reservering", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LoadReserveringen();
        SetStatus(result.Bericht);
    }

    private ApiResult UpdateReservering(int reserveringId, Reservering reservering)
    {
        ApiResult result = _api.UpdateReservering(reserveringId, reservering);
        result.Id = reserveringId;
        return result;
    }

    private void DeleteReservering()
    {
        int? id = GetSelectedReserveringId();
        if (id == null)
        {
            return;
        }

        if (MessageBox.Show(this, "Reservering verwijderen?", "Reservering", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        _api.DeleteReservering(id.Value);
        LoadReserveringen();
        SetStatus("Reservering verwijderd.");
    }

    private void OpenBestellingFromSelection()
    {
        int? id = GetSelectedReserveringId();
        if (id == null)
        {
            return;
        }

        OpenBestelling(id.Value);
    }

    private void OpenBestelling(int reserveringId)
    {
        Reservering? reservering = _api.GetReservering(reserveringId);
        if (reservering == null)
        {
            SetStatus("Onbekende reservering.");
            return;
        }

        using BestellingDialog dialog = new BestellingDialog(_api, reservering);
        dialog.ShowDialog(this);
        LoadOverzicht();
    }

    private void LoadOverzicht()
    {
        bool kok = CurrentOverzichtSoort() == "kok";
        List<OverzichtRegel> overzichtRegels;

        if (kok)
        {
            overzichtRegels = _api.GetOverzicht("kok");
        }
        else
        {
            overzichtRegels = _api.GetOverzicht("ober");
        }

        List<OverzichtRow> rows = new List<OverzichtRow>();
        foreach (OverzichtRegel regel in overzichtRegels)
        {
            OverzichtRow row = new OverzichtRow();
            row.Tafel = regel.Tafel;
            row.Aantal = regel.Aantal;
            row.Item = regel.MenuItemNaam;
            rows.Add(row);
        }

        _overzichtGrid.DataSource = new BindingList<OverzichtRow>(rows);
        if (_overzichtGrid.Columns[nameof(OverzichtRow.Item)] != null)
        {
            if (kok)
            {
                _overzichtGrid.Columns[nameof(OverzichtRow.Item)]!.HeaderText = "Gerecht";
                SetStatus("Overzicht voor kok geladen.");
            }
            else
            {
                _overzichtGrid.Columns[nameof(OverzichtRow.Item)]!.HeaderText = "Drank";
                SetStatus("Overzicht voor ober geladen.");
            }
        }
    }

    private void LoadGegevens()
    {
        string soort = CurrentGegevensSoort();

        switch (soort)
        {
            case "drinken":
                _gegevensGrid.DataSource = new BindingList<MenuItemRow>(ToMenuItemRows(_api.GetMenuItems("drinken")));
                break;
            case "eten":
                _gegevensGrid.DataSource = new BindingList<MenuItemRow>(ToMenuItemRows(_api.GetMenuItems("eten")));
                break;
            case "klanten":
                _gegevensGrid.DataSource = new BindingList<KlantRow>(ToKlantRows(_api.GetKlanten()));
                break;
            case "gerechten":
                _gegevensGrid.DataSource = new BindingList<GerechtRow>(ToGerechtRows(_api.GetGerechten()));
                break;
            case "subgerechten":
                _gegevensGrid.DataSource = new BindingList<SubgerechtRow>(ToSubgerechtRows(_api.GetSubgerechten()));
                break;
            default:
                _gegevensGrid.DataSource = null;
                break;
        }

        HideColumn(_gegevensGrid, "Id");
        SetStatus("Gegevens geladen.");
    }

    private List<MenuItemRow> ToMenuItemRows(List<MenuItem> items)
    {
        Dictionary<string, string> subgerechtLookup = MaakSubgerechtLookup();
        List<MenuItemRow> rows = new List<MenuItemRow>();

        foreach (MenuItem item in items)
        {
            MenuItemRow row = new MenuItemRow();
            row.Code = item.MenuItemCode;
            row.Omschrijving = item.MenuItemNaam;
            row.Prijs = item.Prijs.ToString("0.00", DutchCulture);
            row.ValtOnder = item.SubgerechtCode;
            row.ValtOnderCode = item.SubgerechtCode;

            if (subgerechtLookup.ContainsKey(item.SubgerechtCode))
            {
                row.ValtOnder = subgerechtLookup[item.SubgerechtCode];
            }

            rows.Add(row);
        }

        return rows;
    }

    private static List<KlantRow> ToKlantRows(List<Klant> klanten)
    {
        List<KlantRow> rows = new List<KlantRow>();

        foreach (Klant klant in klanten)
        {
            KlantRow row = new KlantRow();
            row.Id = klant.KlantId;
            row.Naam = klant.KlantNaam;
            row.Telefoon = klant.Telefoon;
            row.Email = klant.Email;
            rows.Add(row);
        }

        return rows;
    }

    private static List<GerechtRow> ToGerechtRows(List<Gerecht> gerechten)
    {
        List<GerechtRow> rows = new List<GerechtRow>();

        foreach (Gerecht gerecht in gerechten)
        {
            GerechtRow row = new GerechtRow();
            row.Code = gerecht.GerechtCode;
            row.Omschrijving = gerecht.GerechtNaam;
            rows.Add(row);
        }

        return rows;
    }

    private List<SubgerechtRow> ToSubgerechtRows(List<Subgerecht> items)
    {
        Dictionary<string, string> gerechtLookup = MaakGerechtLookup();
        List<SubgerechtRow> rows = new List<SubgerechtRow>();

        foreach (Subgerecht item in items)
        {
            SubgerechtRow row = new SubgerechtRow();
            row.Code = item.SubgerechtCode;
            row.Omschrijving = item.SubgerechtNaam;
            row.ValtOnder = item.GerechtCode;
            row.ValtOnderCode = item.GerechtCode;

            if (gerechtLookup.ContainsKey(item.GerechtCode))
            {
                row.ValtOnder = gerechtLookup[item.GerechtCode];
            }

            rows.Add(row);
        }

        return rows;
    }

    private Dictionary<string, string> MaakSubgerechtLookup()
    {
        Dictionary<string, string> lookup = new Dictionary<string, string>();
        List<Subgerecht> subgerechten = _api.GetSubgerechten();

        foreach (Subgerecht subgerecht in subgerechten)
        {
            lookup[subgerecht.SubgerechtCode] = subgerecht.SubgerechtNaam;
        }

        return lookup;
    }

    private Dictionary<string, string> MaakGerechtLookup()
    {
        Dictionary<string, string> lookup = new Dictionary<string, string>();
        List<Gerecht> gerechten = _api.GetGerechten();

        foreach (Gerecht gerecht in gerechten)
        {
            lookup[gerecht.GerechtCode] = gerecht.GerechtNaam;
        }

        return lookup;
    }

    private void EditGegevens(string? key)
    {
        string soort = CurrentGegevensSoort();
        using GegevensDialog? dialog = MaakGegevensDialog(soort, key);

        if (dialog == null)
        {
            return;
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        switch (soort)
        {
            case "drinken":
            case "eten":
                if (key == null)
                {
                    _api.AddMenuItem(dialog.ToMenuItem());
                }
                else
                {
                    _api.UpdateMenuItem(key, dialog.ToMenuItem());
                }
                break;
            case "klanten":
                if (key == null)
                {
                    _api.AddKlant(dialog.ToKlant());
                }
                else
                {
                    _api.UpdateKlant(int.Parse(key, CultureInfo.InvariantCulture), dialog.ToKlant());
                }
                break;
            case "gerechten":
                if (key == null)
                {
                    _api.AddGerecht(dialog.ToGerecht());
                }
                else
                {
                    _api.UpdateGerecht(key, dialog.ToGerecht());
                }
                break;
            case "subgerechten":
                if (key == null)
                {
                    _api.AddSubgerecht(dialog.ToSubgerecht());
                }
                else
                {
                    _api.UpdateSubgerecht(key, dialog.ToSubgerecht());
                }
                break;
        }

        LoadGegevens();
        LoadOverzicht();
    }

    private GegevensDialog? MaakGegevensDialog(string soort, string? key)
    {
        if (soort == "drinken" || soort == "eten")
        {
            string code = "";
            if (key != null)
            {
                code = key;
            }

            MenuItem? menuItem = _api.GetMenuItem(code);
            List<Subgerecht> subgerechten = _api.GetSubgerechten();
            return GegevensDialog.ForMenuItem(menuItem, subgerechten);
        }

        if (soort == "klanten")
        {
            Klant? klant = null;
            if (key != null)
            {
                int klantId = int.Parse(key, CultureInfo.InvariantCulture);
                klant = _api.GetKlant(klantId);
            }

            return GegevensDialog.ForKlant(klant);
        }

        if (soort == "gerechten")
        {
            Gerecht? gerecht = null;
            if (key != null)
            {
                gerecht = _api.GetGerecht(key);
            }

            return GegevensDialog.ForGerecht(gerecht);
        }

        if (soort == "subgerechten")
        {
            Subgerecht? subgerecht = null;
            if (key != null)
            {
                subgerecht = _api.GetSubgerecht(key);
            }

            List<Gerecht> gerechten = _api.GetGerechten();
            return GegevensDialog.ForSubgerecht(subgerecht, gerechten);
        }

        return null;
    }

    private void DeleteGegevens()
    {
        string? key = GetSelectedGegevensKey();
        if (key == null)
        {
            return;
        }

        try
        {
            switch (CurrentGegevensSoort())
            {
                case "drinken":
                case "eten":
                    _api.DeleteMenuItem(key);
                    break;
                case "klanten":
                    _api.DeleteKlant(int.Parse(key, CultureInfo.InvariantCulture));
                    break;
                case "gerechten":
                    _api.DeleteGerecht(key);
                    break;
                case "subgerechten":
                    _api.DeleteSubgerecht(key);
                    break;
            }

            LoadGegevens();
            LoadOverzicht();
            SetStatus("Gegevens verwijderd.");
        }
        catch (ApiUnavailableException)
        {
            throw;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "De gegevens zijn NIET verwijderd.\r\n\r\n" + ex.Message, "Gegevens", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private int? GetSelectedReserveringId()
    {
        if (_reserveringenGrid.CurrentRow == null)
        {
            return null;
        }

        ReserveringRow? row = _reserveringenGrid.CurrentRow.DataBoundItem as ReserveringRow;
        if (row == null)
        {
            return null;
        }

        return row.Id;
    }

    private string? GetSelectedGegevensKey()
    {
        if (_gegevensGrid.CurrentRow == null)
        {
            return null;
        }

        object? data = _gegevensGrid.CurrentRow.DataBoundItem;

        MenuItemRow? menuItem = data as MenuItemRow;
        if (menuItem != null)
        {
            return menuItem.Code;
        }

        KlantRow? klant = data as KlantRow;
        if (klant != null)
        {
            return klant.Id.ToString(CultureInfo.InvariantCulture);
        }

        GerechtRow? gerecht = data as GerechtRow;
        if (gerecht != null)
        {
            return gerecht.Code;
        }

        SubgerechtRow? subgerecht = data as SubgerechtRow;
        if (subgerecht != null)
        {
            return subgerecht.Code;
        }

        return null;
    }

    private string CurrentGegevensSoort()
    {
        SoortItem? item = _gegevensSoort.SelectedItem as SoortItem;
        if (item == null)
        {
            return "drinken";
        }

        return item.Value;
    }

    private string CurrentOverzichtSoort()
    {
        SoortItem? item = _overzichtSoort.SelectedItem as SoortItem;
        if (item == null)
        {
            return "kok";
        }

        return item.Value;
    }

    private void SetStatus(string text)
    {
        _status.Text = text;
    }

    private bool TryInitializeApi()
    {
        try
        {
            _api.CheckStatus();
            return true;
        }
        catch (ApiUnavailableException)
        {
            ShowApiUnavailableMessage();
            return false;
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
            SetStatus("API niet bereikbaar.");
            ShowApiUnavailableMessage();
        }
    }

    private void ShowApiUnavailableMessage()
    {
        MessageBox.Show(this, ApiUnavailableMessage, "API niet bereikbaar", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void ColorReserveringRow(int rowIndex)
    {
        if (rowIndex < 0)
        {
            return;
        }

        ReserveringRow? row = _reserveringenGrid.Rows[rowIndex].DataBoundItem as ReserveringRow;
        if (row == null)
        {
            return;
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        Color color;

        if (row.DatumWaarde < today)
        {
            color = Color.FromArgb(253, 191, 191);
        }
        else if (row.DatumWaarde == today)
        {
            color = Color.FromArgb(233, 255, 229);
        }
        else
        {
            color = Color.FromArgb(253, 224, 165);
        }

        _reserveringenGrid.Rows[rowIndex].DefaultCellStyle.BackColor = color;
    }

    private static string? FindRestaurantImage()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            string candidate = Path.Combine(directory.FullName, "src", "ExcellentTaste.Web", "wwwroot", "images", "Restaurant.jpg");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

}
