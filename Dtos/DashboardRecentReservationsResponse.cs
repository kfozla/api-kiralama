namespace api_kiralama.Dtos;

public class DashboardRecentReservationResponse
{
    public int Id { get; set; }

    public int EmlakId { get; set; }
    public string EmlakAdi { get; set; } = "";

    public string MusteriAdi { get; set; } = "";

    public DateOnly GirisTarihi { get; set; }
    public DateOnly CikisTarihi { get; set; }

    public decimal ToplamFiyat { get; set; }

    public int Durum { get; set; }

    public DateTime CreatedAt { get; set; }
}