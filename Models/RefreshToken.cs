namespace api_kiralama.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; } = DateTime.UtcNow;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; }
}