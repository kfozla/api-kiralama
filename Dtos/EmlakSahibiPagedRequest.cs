namespace api_kiralama.Dtos;

public class EmlakSahibiPagedRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
}