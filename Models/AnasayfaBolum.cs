namespace api_kiralama.Models;

public class AnasayfaBolum
{
    public int Id { get; set; }

    public string Baslik { get; set; } = string.Empty;
    public string? Aciklama { get; set; }
    public string Slug { get; set; } = string.Empty;

    public AnasayfaGosterimTipi GosterimTipi { get; set; }
        = AnasayfaGosterimTipi.Slider;

    public AnasayfaSecimTipi SecimTipi { get; set; }
        = AnasayfaSecimTipi.Manuel;

    public int Sira { get; set; }
    public int MaksimumVillaSayisi { get; set; } = 10;
    public bool Aktif { get; set; } = true;

    public List<AnasayfaBolumEmlak> Emlaklar { get; set; } = new();
}