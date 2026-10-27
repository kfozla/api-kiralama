namespace api_kiralama.Dtos;

public class EmlakTakvimResponse
{
    public int EmlakId { get; set; }

    public int Yil { get; set; }

    public int Ay { get; set; }

    public List<EmlakTakvimGunResponse> Gunler { get; set; } = new();
}