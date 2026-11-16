namespace api_kiralama.Dtos;

public class EmlakPaylasimDetayResponse
{
    public string? Baslik { get; set; }

    public DateOnly? GirisTarihi { get; set; }
    public DateOnly? CikisTarihi { get; set; }

    public DateTime ExpiresAt { get; set; }

    public List<EmlakPaylasimEmlakResponse> Emlaklar { get; set; }
        = new();
}

public class EmlakPaylasimEmlakResponse
{
    public int Id { get; set; }
    public string? KapakFotografUrl { get; set; }
    public string EmlakAdi { get; set; } = string.Empty;

    public int BolgeId { get; set; }

    public int Kapasite { get; set; }
    public int YatakOdasiSayisi { get; set; }
    public int BanyoSayisi { get; set; }
}