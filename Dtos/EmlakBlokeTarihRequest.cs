using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class EmlakBlokeTarihRequest
{
    [Range(1, int.MaxValue)]
    public int EmlakId { get; set; }

    public DateOnly BaslangicTarihi { get; set; }
    public DateOnly BitisTarihi { get; set; }

    public string? Aciklama { get; set; }
}