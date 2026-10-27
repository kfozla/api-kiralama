namespace api_kiralama.Models;

public class IsletmeAyarlari
{
    public int Id { get; set; }

    public string IsletmeAdi { get; set; } = "";
    public string? YetkiliKisi { get; set; }

    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }
    public string? WebSitesi { get; set; }

    // Özel depolama alanındaki dosya adı
    public string? LogoUrl { get; set; }

    public bool TursabUyeligiVar { get; set; }
    public string? TursabBelgeNo { get; set; }
    public string? TursabBelgeUrl { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}