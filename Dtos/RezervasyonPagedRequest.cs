namespace api_kiralama.Dtos;

public class RezervasyonPagedRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public int? Durum { get; set; }

    public DateOnly? GirisBaslangic { get; set; }

    public DateOnly? GirisBitis { get; set; }
}