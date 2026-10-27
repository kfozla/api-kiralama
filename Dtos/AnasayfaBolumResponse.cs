using api_kiralama.Models;

namespace api_kiralama.Dtos;

public class AnasayfaBolumResponse
{
    public int Id { get; set; }

    public string Baslik { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string Slug { get; set; } = string.Empty;

    public AnasayfaGosterimTipi GosterimTipi { get; set; }

    public AnasayfaSecimTipi SecimTipi { get; set; }

    public int Sira { get; set; }
    public bool Aktif { get; set; }

    public int MaksimumVillaSayisi { get; set; }

    public List<int> EmlakIds { get; set; } = new();
}