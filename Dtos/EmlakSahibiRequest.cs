using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class EmlakSahibiRequest
{
    [Required]
    public string Ad { get; set; } = string.Empty;

    [Required]
    public string Soyad { get; set; } = string.Empty;

    [Required]
    public string Telefon { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public string? Aciklama { get; set; }
}