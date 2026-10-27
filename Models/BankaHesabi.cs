namespace api_kiralama.Models;

public class BankaHesabi
{
    public int Id { get; set; }

    public string BankaAdi { get; set; } = string.Empty;

    public string HesapSahibi { get; set; } = string.Empty;

    public string Iban { get; set; } = string.Empty;

    public string? SubeAdi { get; set; }

    public string ParaBirimi { get; set; } = "TRY";

    // Örn: /uploads/bankalar/uuid.png
    public string? LogoUrl { get; set; }

    public int Sira { get; set; }

    public bool AktifMi { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}