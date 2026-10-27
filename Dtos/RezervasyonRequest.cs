using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class RezervasyonRequest
{
    [Range(1, int.MaxValue)]
    public int EmlakId { get; set; }

    [Required]
    public string MusteriAd { get; set; } = string.Empty;

    [Required]
    public string MusteriSoyad { get; set; } = string.Empty;

    [Required]
    public string Telefon { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public DateOnly GirisTarihi { get; set; }
    public DateOnly CikisTarihi { get; set; }

    [Range(1, int.MaxValue)]
    public int KisiSayisi { get; set; }
}