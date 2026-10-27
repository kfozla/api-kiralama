using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class BolgeRequest
{
    [Required]
    public string Ad { get; set; } = string.Empty;

    public int? ParentBolgeId { get; set; }
}