namespace api_kiralama.Models;

public class EmlakPaylasim
{
    public int Id { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public string? Baslik { get; set; }

    public DateOnly? GirisTarihi { get; set; }
    public DateOnly? CikisTarihi { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public List<EmlakPaylasimItem> Items { get; set; } = new();
}

public class EmlakPaylasimItem
{
    public int Id { get; set; }

    public int EmlakPaylasimId { get; set; }
    public EmlakPaylasim EmlakPaylasim { get; set; } = null!;

    public int EmlakId { get; set; }
    public Emlak Emlak { get; set; } = null!;

    public int Sira { get; set; }
}