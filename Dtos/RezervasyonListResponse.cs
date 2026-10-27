namespace api_kiralama.Dtos;

public class RezervasyonListResponse
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

    // Konaklama + temizlik ücreti
    public decimal ToplamFiyat { get; set; }

    // Rezervasyon oluşturulurken uygulanan ücret
    public decimal TemizlikUcreti { get; set; }

    // Girişte nakit alınacak, toplam fiyata dahil değil
    public decimal HasarDepozitosu { get; set; }

    public int Durum { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}