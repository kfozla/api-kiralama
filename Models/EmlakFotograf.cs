namespace api_kiralama.Models;

public class EmlakFotograf
{
    public int Id { get; set; }

    public int EmlakId { get; set; }

    public string DosyaYolu { get; set; } = string.Empty;

    public int Sira { get; set; } = 0;

    public bool KapakMi { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}