using api_kiralama.Repositories;
using api_kiralama.Dtos;

namespace api_kiralama.Services;

public class EmlakTakvimService
{
    private readonly EmlakTakvimRepository _repository;

    public EmlakTakvimService(EmlakTakvimRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmlakTakvimResponse?> GetTakvimAsync(
        int emlakId,
        int yil,
        int ay)
    {
        if (yil < 1 || yil > 9999 || ay < 1 || ay > 12)
            throw new ArgumentException("Geçersiz yıl veya ay.");

        bool emlakVar = await _repository.EmlakExistsAsync(emlakId);

        if (!emlakVar)
            return null;

        var baslangic = new DateOnly(yil, ay, 1);

        // Aralık sonunu özel olarak ele alıyoruz.
        var bitis = baslangic.AddMonths(1);

        var fiyatlar = await _repository.GetFiyatlarAsync(
            emlakId, baslangic, bitis);

        var rezervasyonlar = await _repository.GetRezervasyonlarAsync(
            emlakId, baslangic, bitis);

        var blokeler = await _repository.GetBlokelerAsync(
            emlakId, baslangic, bitis);

        // Türkiye yerel tarihi
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(
            "Europe/Istanbul");

        var bugun = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow, timezone));

        var response = new EmlakTakvimResponse
        {
            EmlakId = emlakId,
            Yil = yil,
            Ay = ay
        };

        int gunSayisi = DateTime.DaysInMonth(yil, ay);

        for (int i = 1; i <= gunSayisi; i++)
        {
            var tarih = new DateOnly(yil, ay, i);

            bool dolu = rezervasyonlar.Any(r =>
                r.GirisTarihi <= tarih &&
                r.CikisTarihi > tarih);

            bool kapali = blokeler.Any(b =>
                b.BaslangicTarihi <= tarih &&
                b.BitisTarihi > tarih);

            var gunFiyatlari = fiyatlar
                .Where(f =>
                    f.BaslangicTarihi <= tarih &&
                    f.BitisTarihi > tarih)
                .ToList();

            decimal? gecelikFiyat =
                gunFiyatlari.Count == 1
                    ? gunFiyatlari[0].GecelikFiyat
                    : null;

            string durum;

            if (tarih < bugun)
            {
                durum = "Gecmis";
            }
            else if (dolu)
            {
                durum = "Dolu";
            }
            else if (kapali)
            {
                durum = "Kapali";
            }
            else if (gecelikFiyat == null)
            {
                durum = "FiyatTanimsiz";
            }
            else
            {
                durum = "Musait";
            }

            response.Gunler.Add(new EmlakTakvimGunResponse
            {
                Tarih = tarih,
                Durum = durum,
                GecelikFiyat = gecelikFiyat
            });
        }

        return response;
    }
}