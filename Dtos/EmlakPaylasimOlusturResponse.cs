namespace api_kiralama.Dtos;

public class EmlakPaylasimOlusturResponse
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}