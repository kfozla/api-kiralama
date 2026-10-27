namespace api_kiralama.Models;

public class Bolge
{ 
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty; 
    public int? ParentBolgeId { get; set; }
}