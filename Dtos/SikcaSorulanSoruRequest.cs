using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class SikcaSorulanSoruRequest
{
    [Required]
    [MaxLength(500)]
    public string Soru { get; set; } = string.Empty;

    [Required]
    public string CevapHtml { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Sira { get; set; }

    public bool AktifMi { get; set; } = true;
}