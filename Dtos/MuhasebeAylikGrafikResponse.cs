namespace api_kiralama.Dtos;

public class MuhasebeAylikGrafikResponse
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public decimal RezervasyonHacmi { get; set; }
    public decimal OnOdeme { get; set; }
    public decimal Komisyon { get; set; }
    public decimal EvSahibiHakedis { get; set; }
}