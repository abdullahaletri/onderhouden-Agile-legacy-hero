using ExcellentTaste.Core.Models;
using ExcellentTaste.Core.Repositories;

namespace ExcellentTaste.Core.Services;

public class BestellingService
{
    private readonly BestellingRepository _bestellingen;
    private readonly MenuRepository _menu;
    private readonly ReserveringRepository _reserveringen;

    public BestellingService(BestellingRepository bestellingen, MenuRepository menu, ReserveringRepository reserveringen)
    {
        _bestellingen = bestellingen;
        _menu = menu;
        _reserveringen = reserveringen;
    }

    public List<Bestelling> GetByReservering(int reserveringId) => _bestellingen.GetByReservering(reserveringId);

    public bool AddItem(int reserveringId, string menuItemCode)
    {
        var reservering = _reserveringen.GetById(reserveringId);
        var menuItem = _menu.GetByCode(menuItemCode);
        if (reservering is null || menuItem is null)
        {
            return false;
        }

        _bestellingen.AddItem(reserveringId, reservering.Tafel, menuItemCode, menuItem.Prijs);
        return true;
    }

    public void PlusItem(int reserveringId, string menuItemCode) => _bestellingen.ChangeAantal(reserveringId, menuItemCode, 1);

    public void MinItem(int reserveringId, string menuItemCode) => _bestellingen.ChangeAantal(reserveringId, menuItemCode, -1);

    public void DeleteItem(int reserveringId, string menuItemCode) => _bestellingen.DeleteItem(reserveringId, menuItemCode);

    public List<BonRegel> GetBon(int reserveringId)
    {
        return GetByReservering(reserveringId)
            .Select(item => new BonRegel
            {
                MenuItemNaam = item.MenuItemNaam,
                Aantal = item.Aantal,
                Prijs = item.Prijs
            })
            .ToList();
    }
}
