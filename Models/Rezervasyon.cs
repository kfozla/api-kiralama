namespace api_kiralama.Models;

public class Rezervasyon
{
    public int Id { get; set; }
    public int EmlakId { get; set; }

    public string MusteriAd { get; set; } = string.Empty;
    public string MusteriSoyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string? Email { get; set; }

    public DateOnly GirisTarihi { get; set; }
    public DateOnly CikisTarihi { get; set; }

    public int KisiSayisi { get; set; }

    // Konaklama + uygulanan temizlik ücreti
    public decimal ToplamFiyat { get; set; }

    // Rezervasyon oluşturulurken uygulanan temizlik ücreti
    public decimal TemizlikUcreti { get; set; } = 0;

    // Girişte nakit alınacak, toplam fiyata dahil değil
    public decimal HasarDepozitosu { get; set; } = 0;

    public RezervasyonDurumu Durum { get; set; }
        = RezervasyonDurumu.Beklemede;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}