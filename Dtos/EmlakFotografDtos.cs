using Microsoft.AspNetCore.Http;

namespace api_kiralama.Dtos;

public class EmlakFotografResponse
{
    public int Id { get; set; }
    public int EmlakId { get; set; }
    public string DosyaYolu { get; set; } = "";
    public int Sira { get; set; }
    public bool KapakMi { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EmlakFotografUploadRequest
{
    public List<IFormFile> Files { get; set; } = new();
}

public class EmlakFotografSiralamaRequest
{
    public List<int> FotografIds { get; set; } = new();
}