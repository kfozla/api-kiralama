using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class SayfaIcerigiUpdateRequest
{
    [Required]
    [MaxLength(200)]
    public string Baslik { get; set; } = string.Empty;

    [Required]
    public string IcerikHtml { get; set; } = string.Empty;
}