namespace api_kiralama.Dtos;

public class MuhasebeRezervasyonResponse
{
    public int Id { get; set; }
    public int EmlakId { get; set; }
    public string EmlakAdi { get; set; } = string.Empty;

    public string MusteriAd { get; set; } = string.Empty;
    public string MusteriSoyad { get; set; } = string.Empty;

    public DateOnly GirisTarihi { get; set; }
    public DateOnly CikisTarihi { get; set; }

    public decimal ToplamFiyat { get; set; }
    public decimal TemizlikUcreti { get; set; }
    public decimal OnOdemeTutari { get; set; }
    public decimal KomisyonTutari { get; set; }

    // SQL'de hesaplanacak alanlar
    public decimal GiristeOdenecek { get; set; }
    public decimal EmlakSahibiHakedis { get; set; }
    public decimal AcenteninBorcu { get; set; }
    public decimal KomisyonAlacagi { get; set; }
}