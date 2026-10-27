namespace api_kiralama.Models;

public class EmlakFiyat
{
    public int Id { get; set; }
    public int EmlakId { get; set; }

    public DateOnly BaslangicTarihi { get; set; }
    public DateOnly BitisTarihi { get; set; }

    public decimal GecelikFiyat { get; set; }
}