using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class AnasayfaBolumEmlaklarRequest
{
    [Required]
    public List<AnasayfaBolumEmlakRequest> Emlaklar { get; set; }
        = new();
}