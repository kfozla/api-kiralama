namespace api_kiralama.Dtos;

public class DashboardSummaryResponse
{
    public int ToplamEmlak { get; set; }
    public int AktifRezervasyon { get; set; }
    public int BekleyenRezervasyon { get; set; }
    public decimal BuAyRezervasyonTutari { get; set; }
}