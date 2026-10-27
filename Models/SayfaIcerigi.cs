namespace api_kiralama.Models;

public class SayfaIcerigi
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Baslik { get; set; } = string.Empty;

    public string IcerikHtml { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}