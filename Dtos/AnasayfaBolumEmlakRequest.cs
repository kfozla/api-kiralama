using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class AnasayfaBolumEmlakRequest
{
    [Required]
    public int EmlakId { get; set; }

    public int Sira { get; set; }
}