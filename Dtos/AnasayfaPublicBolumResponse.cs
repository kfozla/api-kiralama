using api_kiralama.Models;

namespace api_kiralama.Dtos;

public class AnasayfaPublicBolumResponse
{
    public int Id { get; set; }

    public string Baslik { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Aciklama { get; set; }

    public AnasayfaGosterimTipi GosterimTipi { get; set; }

    public int Sira { get; set; }

    public List<AnasayfaPublicEmlakResponse> Emlaklar { get; set; }
        = new();
}