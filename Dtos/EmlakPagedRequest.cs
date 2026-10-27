namespace api_kiralama.Dtos;

public class EmlakPagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public List<int> BolgeIds { get; set; } = new();
    public int? EmlakTipi { get; set; }
    public int? MinKapasite { get; set; }

    public List<int> OzellikIds { get; set; } = new();
}