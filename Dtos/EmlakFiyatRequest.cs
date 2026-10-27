using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class EmlakFiyatRequest
{
    [Range(1, int.MaxValue)]
    public int EmlakId { get; set; }

    public DateOnly BaslangicTarihi { get; set; }
    public DateOnly BitisTarihi { get; set; }

    [Range(typeof(decimal), "0.01", "999999999",
     ParseLimitsInInvariantCulture = true)]
    public decimal GecelikFiyat { get; set; }
}