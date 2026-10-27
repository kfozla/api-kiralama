namespace api_kiralama.Dtos;

public class RezervasyonStatsResponse
{
    public int ToplamRezervasyon { get; set; }

    public int Bekleyen { get; set; }

    public int Onaylanan { get; set; }

    public int IptalEdilen { get; set; }

    public int Tamamlanan { get; set; }

    public decimal ToplamGelir { get; set; }
}