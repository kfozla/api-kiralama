using api_kiralama.Models;
using System.ComponentModel.DataAnnotations;

namespace api_kiralama.Dtos;

public class AnasayfaBolumRequest
{
    [Required]
    [MaxLength(150)]
    public string Baslik { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Aciklama { get; set; }

    [Required]
    [MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    public AnasayfaGosterimTipi GosterimTipi { get; set; }

    public AnasayfaSecimTipi SecimTipi { get; set; }

    [Range(1, 100)]
    public int MaksimumVillaSayisi { get; set; } = 10;

    public int Sira { get; set; }

    public bool Aktif { get; set; } = true;
}