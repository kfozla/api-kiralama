namespace api_kiralama.Dtos;

public class AnasayfaPublicEmlakResponse
{
    public int Id { get; set; }

    public string EmlakAdi { get; set; } = string.Empty;

    public int BolgeId { get; set; }

    public int Kapasite { get; set; }

    public int YatakOdasiSayisi { get; set; }

    public int BanyoSayisi { get; set; }
}