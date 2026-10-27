namespace api_kiralama.Models;

public class AnasayfaBolumEmlak
{
    public int Id { get; set; }

    public int AnasayfaBolumId { get; set; }
    public AnasayfaBolum AnasayfaBolum { get; set; } = null!;

    public int EmlakId { get; set; }

    public int Sira { get; set; }
}