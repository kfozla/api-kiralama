using api_kiralama.Models;

namespace api_kiralama.Dtos;

public class AdvancedSearchRequest
{
    public DateOnly? Giris { get; set; }
    public DateOnly? Cikis { get; set; }

    public EmlakTipi? EmlakTipi { get; set; }
    public int? BolgeId { get; set; }

    public int? MinKapasite { get; set; }
    public int? MinYatakOdasi { get; set; }
    public int? MinBanyo { get; set; }

    public List<int>? OzellikIds { get; set; }

    public HavuzKorunakTipi? HavuzKorunakTipi { get; set; }
}