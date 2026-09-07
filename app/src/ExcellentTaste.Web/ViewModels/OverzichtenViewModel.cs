using ExcellentTaste.Core.Models;

namespace ExcellentTaste.Web.ViewModels;

public class OverzichtenViewModel
{
    public string Overzicht { get; set; } = "";

    public string Titel { get; set; } = "";

    public string ItemKolom { get; set; } = "";

    public string ApiFout { get; set; } = "";

    public List<OverzichtRegel> Regels { get; set; } = [];
}
