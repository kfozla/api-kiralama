namespace api_kiralama.Models;

public class EmlakSahibi
{
    public int Id { get; set; }

    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;

    public string Telefon { get; set; } = string.Empty;
    public string? Email { get; set; }

    public string? Aciklama { get; set; }

    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

    public List<Emlak> Emlaklar { get; set; } = new();
    
}