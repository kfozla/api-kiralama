namespace api_kiralama.Dtos;

public class EmlakSahibiListResponse
{
    public int Id { get; set; }

    public string Ad { get; set; } = string.Empty;

    public string Soyad { get; set; } = string.Empty;
    public int EmlakSayisi { get; set; }

    public string Telefon { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Aciklama { get; set; }

    public DateTime KayitTarihi { get; set; }
}