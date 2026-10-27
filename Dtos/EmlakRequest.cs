using System.ComponentModel.DataAnnotations;
using api_kiralama.Models;

namespace api_kiralama.Dtos;

public class EmlakRequest
{
    [Range(1, int.MaxValue)]
    public int? EmlakSahibiId { get; set; }

    [EnumDataType(typeof(EmlakTipi))]
    public EmlakTipi EmlakTipi { get; set; }

    [Range(1, int.MaxValue)]
    public int BolgeId { get; set; }

    [Required]
    public string EmlakAdı { get; set; } = string.Empty;

    public string Açıklama { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Kapasite { get; set; }

    [Range(0, int.MaxValue)]
    public int YatakOdasıSayısı { get; set; }

    [Range(0, int.MaxValue)]
    public int BanyoSayısı { get; set; }

    public string GenelBilgiler { get; set; } = string.Empty;

    public List<int> OzellikIds { get; set; } = new();
    public List<int> OneCikanOzellikIds { get; set; } = new();
    public List<Havuz>? HavuzBilgileri { get; set; } = new();

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }
    
    public TimeOnly GirisSaati { get; set; } = new(16, 0);
    public TimeOnly CikisSaati { get; set; } = new(10, 0);

    [Range(typeof(decimal), "0", "999999999")]
    public decimal HasarDepozitosu { get; set; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal TemizlikUcreti { get; set; }

    [Range(1, 365)]
    public int MinimumKonaklamaSuresi { get; set; } = 1;
    [Range(0, 365)]
    public int MinimumKonaklamaTemizlik { get; set; } = 0;
    
    public bool Yayinda { get; set; } = false;

    [Range(0, 100)]
    public int OtoparkSayisi { get; set; } = 0;

    [MaxLength(100)]
    public string? BelgeNo { get; set; }
}