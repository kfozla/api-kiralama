namespace api_kiralama.Models;

public class SikcaSorulanSoru
{
    public int Id { get; set; }

    public string Soru { get; set; } = string.Empty;

    public string CevapHtml { get; set; } = string.Empty;

    public int Sira { get; set; }

    public bool AktifMi { get; set; } = true;
}