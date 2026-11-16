namespace api_kiralama.Dtos;

public class EmlakPaylasimOlusturRequest
{
    public string? Baslik { get; set; }

    public DateOnly? GirisTarihi { get; set; }
    public DateOnly? CikisTarihi { get; set; }

    public List<int> EmlakIds { get; set; } = new();
}