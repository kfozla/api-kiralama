namespace api_kiralama.Models;

public class EmlakOzellik
{
    public int Id { get; set; }
    public string OzellikAdi { get; set; } = String.Empty;
    public OzellikKategori Kategori { get; set; }
    public List<Emlak> Emlaklar { get; set; } = new();
    public string? LogoUrl { get; set; }
}