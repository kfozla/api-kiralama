namespace api_kiralama.Dtos;

public class IsletmeAyarlariResponse
{
    public int Id { get; set; }
    public string IsletmeAdi { get; set; } = "";
    public string? YetkiliKisi { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }
    public string? WebSitesi { get; set; }

    public bool LogoVar { get; set; }
    public bool TursabUyeligiVar { get; set; }
    public string? TursabBelgeNo { get; set; }
    public bool TursabBelgesiVar { get; set; }

    public DateTime UpdatedAt { get; set; }
}