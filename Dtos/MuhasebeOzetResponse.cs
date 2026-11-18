namespace api_kiralama.Dtos;

public class MuhasebeOzetResponse
{
    public int RezervasyonSayisi { get; set; }

    public decimal ToplamRezervasyonHacmi { get; set; }

    public decimal ToplamOnOdeme { get; set; }

    public decimal ToplamKomisyon { get; set; }

    public decimal ToplamEmlakSahibiHakedis { get; set; }

    public decimal ToplamGiristeOdenecek { get; set; }

    public decimal ToplamAcenteBorcu { get; set; }

    public decimal ToplamKomisyonAlacagi { get; set; }
}