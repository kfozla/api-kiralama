namespace api_kiralama.Dtos;

public class RezervasyonTakvimResponse
{
    public List<RezervasyonTakvimItemResponse> Reservations
    { get; set; } = new();

    public int TodayEntries { get; set; }
    public int TodayExits { get; set; }
    public int PendingCount { get; set; }
}

public class RezervasyonTakvimItemResponse
{
    public int Id { get; set; }

    public int EmlakId { get; set; }
    public string EmlakAdi { get; set; } = string.Empty;

    public string MusteriAd { get; set; } = string.Empty;
    public string MusteriSoyad { get; set; } = string.Empty;

    public string Telefon { get; set; } = string.Empty;
    public string? Email { get; set; }

    public DateOnly GirisTarihi { get; set; }
    public DateOnly CikisTarihi { get; set; }

    public int KisiSayisi { get; set; }
    public int Durum { get; set; }
}