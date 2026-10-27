namespace api_kiralama.Dtos;

public class RezervasyonTakvimRequest
{
    public int Year { get; set; }
    public int Month { get; set; }

    public int? EmlakId { get; set; }

    public bool ShowPending { get; set; } = false;

    // Frontend tarafından İstanbul tarihi gönderilecek.
    public DateOnly Today { get; set; }
}