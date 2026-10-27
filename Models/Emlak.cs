namespace api_kiralama.Models;

public class Emlak
{
    public int Id { get; set; }
    public int? EmlakSahibiId { get; set; }
    public EmlakTipi EmlakTipi { get; set; }
    public int BolgeId { get; set; }

    public string EmlakAdı { get; set; } = string.Empty;
    public string Açıklama { get; set; } = string.Empty;

    public int Kapasite { get; set; }
    public int YatakOdasıSayısı { get; set; }
    public int BanyoSayısı { get; set; }

    public string GenelBilgiler { get; set; } = string.Empty;

    public List<EmlakOzellik> OneCıkanOzellikler { get; set; } = new();
    public List<EmlakOzellik> EmlakOzellikleri { get; set; } = new();
    public List<Havuz>? HavuzBilgileri { get; set; } = new();

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public TimeOnly GirisSaati { get; set; } = new(16, 0);
    public TimeOnly CikisSaati { get; set; } = new(10, 0);
    
    public decimal HasarDepozitosu { get; set; } = 0;
    public decimal TemizlikUcreti { get; set; } = 0;
    public int MinimumKonaklamaSuresi { get; set; } = 1;
    public int MinimumKonaklamaTemizlik { get; set; } = 0;
    
    // Müşteri sitesinde yayın durumu
    public bool Yayinda { get; set; } = false;

// Otopark kapasitesi
    public int OtoparkSayisi { get; set; } = 0;

// Turizm amaçlı kiralama izin belgesi numarası
    public string? BelgeNo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
}