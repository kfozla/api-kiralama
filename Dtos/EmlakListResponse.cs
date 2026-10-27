namespace api_kiralama.Dtos;

public class EmlakListResponse
{
    public int Id { get; set; }

    public string EmlakAdı { get; set; } = "";

    public int BolgeId { get; set; }

    public int EmlakTipi { get; set; }

    public int Kapasite { get; set; }

    public int YatakOdasıSayısı { get; set; }

    public int BanyoSayısı { get; set; }

    public string? KapakFotografUrl { get; set; }
    
    public bool Yayinda { get; set; } 

    
    public int OtoparkSayisi { get; set; }

    
    public string? BelgeNo { get; set; }
}