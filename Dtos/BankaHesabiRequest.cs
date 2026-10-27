using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class BankaHesabiRequest
{
    [Required]
    [MaxLength(150)]
    public string BankaAdi { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string HesapSahibi { get; set; } = string.Empty;

    [Required]
    [MaxLength(34)]
    public string Iban { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? SubeAdi { get; set; }

    [Required]
    [MaxLength(3)]
    public string ParaBirimi { get; set; } = "TRY";

    [Range(0, int.MaxValue)]
    public int Sira { get; set; }

    public bool AktifMi { get; set; } = true;
}