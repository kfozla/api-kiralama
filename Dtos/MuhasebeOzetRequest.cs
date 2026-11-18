namespace api_kiralama.Dtos;

public class MuhasebeOzetRequest
{
    public DateOnly? BaslangicTarihi { get; set; }

    public DateOnly? BitisTarihi { get; set; }

    public int? EmlakId { get; set; }
}