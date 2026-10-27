namespace api_kiralama.Dtos;

public class DashboardMonthlyReservationResponse
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public string AyAdi { get; set; } = "";
    public int RezervasyonSayisi { get; set; }
}