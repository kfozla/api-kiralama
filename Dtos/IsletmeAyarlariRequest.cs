using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class IsletmeAyarlariRequest
{
    [Required]
    [StringLength(200)]
    public string IsletmeAdi { get; set; } = "";

    [StringLength(150)]
    public string? YetkiliKisi { get; set; }

    [StringLength(30)]
    public string? Telefon { get; set; }

    [EmailAddress]
    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(1000)]
    public string? Adres { get; set; }

    [StringLength(500)]
    public string? WebSitesi { get; set; }

    public bool TursabUyeligiVar { get; set; }

    [StringLength(100)]
    public string? TursabBelgeNo { get; set; }
}