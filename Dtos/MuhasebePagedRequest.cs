namespace api_kiralama.Dtos;

public class MuhasebePagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int? EmlakId { get; set; }

    public DateOnly? GirisTarihi { get; set; }
    public DateOnly? CikisTarihi { get; set; }
}