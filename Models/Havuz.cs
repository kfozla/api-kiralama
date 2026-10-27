namespace api_kiralama.Models;

public class Havuz
{
    public String HavuzTipi { get; set; }
    public decimal En {get; set;}
    public decimal Boy {get; set;}
    public decimal Derinlik {get; set;}
    public HavuzKorunakTipi HavuzKorunakTipi {get; set;}
}