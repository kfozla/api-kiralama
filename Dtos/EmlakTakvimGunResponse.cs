namespace api_kiralama.Dtos;

public class EmlakTakvimGunResponse
{
    public DateOnly Tarih { get; set; }

    public string Durum { get; set; } = string.Empty;

    public decimal? GecelikFiyat { get; set; }
}