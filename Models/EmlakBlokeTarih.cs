namespace api_kiralama.Models;

public class EmlakBlokeTarih
{
    public int Id { get; set; }
    public int EmlakId { get; set; }
    
    public int? RezervasyonId { get; set; }

    public DateOnly BaslangicTarihi { get; set; }
    public DateOnly BitisTarihi { get; set; }

    public string? Aciklama { get; set; }
}