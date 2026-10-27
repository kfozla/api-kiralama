using api_kiralama.Models;

namespace api_kiralama.Dtos;

public class EmlakResponse
{
    public int Id { get; set; }
    public int? EmlakSahibiId { get; set; }
    public int BolgeId { get; set; }
    public EmlakTipi EmlakTipi { get; set; }

    public string EmlakAdı { get; set; } = string.Empty;
    public string Açıklama { get; set; } = string.Empty;

    public int Kapasite { get; set; }
    public int YatakOdasıSayısı { get; set; }
    public int BanyoSayısı { get; set; }

    public string GenelBilgiler { get; set; } = string.Empty;

    public List<int> OzellikIds { get; set; } = new();
    public List<int> OneCikanOzellikIds { get; set; } = new();
    public List<Havuz>? HavuzBilgileri { get; set; } = new();

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public TimeOnly GirisSaati { get; set; }
    public TimeOnly CikisSaati { get; set; }

    public decimal HasarDepozitosu { get; set; }
    public decimal TemizlikUcreti { get; set; }

    public int MinimumKonaklamaSuresi { get; set; }
    public int MinimumKonaklamaTemizlik { get; set; }
    public bool Yayinda { get; set; } 
    public int OtoparkSayisi { get; set; } 
    public string? BelgeNo { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}