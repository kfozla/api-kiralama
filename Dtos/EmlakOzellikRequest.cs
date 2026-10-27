using System.ComponentModel.DataAnnotations;
using api_kiralama.Models;

namespace api_kiralama.Dtos;

public class EmlakOzellikRequest
{
    [Required]
    public string OzellikAdi { get; set; } = string.Empty;

    [Required]
    [EnumDataType(typeof(OzellikKategori))]
    public OzellikKategori Kategori { get; set; }
}