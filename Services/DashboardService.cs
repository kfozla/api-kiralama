
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class DashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        // Türkiye saatine göre ay başlangıcı
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(
            "Europe/Istanbul"
        );

        var now = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            timezone
        );

        var monthStartLocal = new DateTime(
            now.Year,
            now.Month,
            1,
            0, 0, 0,
            DateTimeKind.Unspecified
        );

        var nextMonthLocal = monthStartLocal.AddMonths(1);

        // CreatedAt UTC saklandığı için sınırları UTC'ye çeviriyoruz.
        var monthStart = TimeZoneInfo.ConvertTimeToUtc(
            monthStartLocal,
            timezone
        );

        var nextMonth = TimeZoneInfo.ConvertTimeToUtc(
            nextMonthLocal,
            timezone
        );

        var toplamEmlak = await _context.Emlaklar
            .CountAsync(e => !e.IsDeleted);

        var aktifRezervasyon = await _context.Rezervasyonlar
            .CountAsync(r =>
                r.Durum == RezervasyonDurumu.Onaylandi);

        var bekleyenRezervasyon = await _context.Rezervasyonlar
            .CountAsync(r =>
                r.Durum == RezervasyonDurumu.Beklemede);

        var buAyRezervasyonTutari = await _context.Rezervasyonlar
            .Where(r =>
                r.CreatedAt >= monthStart &&
                r.CreatedAt < nextMonth &&
                r.Durum != RezervasyonDurumu.IptalEdildi)
            .SumAsync(r => (decimal?)r.ToplamFiyat) ?? 0m;

        return new DashboardSummaryResponse
        {
            ToplamEmlak = toplamEmlak,
            AktifRezervasyon = aktifRezervasyon,
            BekleyenRezervasyon = bekleyenRezervasyon,
            BuAyRezervasyonTutari = buAyRezervasyonTutari
        };
    }
    
public async Task<List<DashboardMonthlyReservationResponse>>
    GetLastSixMonthsReservationsAsync()
{
    var timezone = TimeZoneInfo.FindSystemTimeZoneById(
        "Europe/Istanbul"
    );

    var now = TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.UtcNow,
        timezone
    );

    // İçinde bulunduğumuz ayın ilk günü
    var currentMonth = new DateTime(
        now.Year,
        now.Month,
        1
    );

    // Güncel ay dahil geriye doğru 6 ay
    var firstMonth = currentMonth.AddMonths(-5);

    // UTC tarih aralığı
    var startUtc = TimeZoneInfo.ConvertTimeToUtc(
        DateTime.SpecifyKind(
            firstMonth,
            DateTimeKind.Unspecified
        ),
        timezone
    );

    var endUtc = TimeZoneInfo.ConvertTimeToUtc(
        DateTime.SpecifyKind(
            currentMonth.AddMonths(1),
            DateTimeKind.Unspecified
        ),
        timezone
    );

    // Yalnızca ilgili tarih aralığındaki rezervasyonları çek
    var rezervasyonTarihleri = await _context.Rezervasyonlar
        .AsNoTracking()
        .Where(r =>
            r.CreatedAt >= startUtc &&
            r.CreatedAt < endUtc &&
            r.Durum != RezervasyonDurumu.IptalEdildi)
        .Select(r => r.CreatedAt)
        .ToListAsync();

    var culture = new System.Globalization.CultureInfo("tr-TR");

    var result = new List<DashboardMonthlyReservationResponse>();

    for (int i = 0; i < 6; i++)
    {
        var month = firstMonth.AddMonths(i);
        var nextMonth = month.AddMonths(1);

        var monthStartUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(
                month,
                DateTimeKind.Unspecified
            ),
            timezone
        );

        var monthEndUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(
                nextMonth,
                DateTimeKind.Unspecified
            ),
            timezone
        );

        var count = rezervasyonTarihleri.Count(date =>
            date >= monthStartUtc &&
            date < monthEndUtc
        );

        result.Add(new DashboardMonthlyReservationResponse
        {
            Yil = month.Year,
            Ay = month.Month,
            AyAdi = culture.DateTimeFormat.GetMonthName(month.Month),
            RezervasyonSayisi = count
        });
    }

    return result;
}


public async Task<List<DashboardRecentReservationResponse>>
    GetRecentReservationsAsync()
{
    // Son 5 rezervasyonu veritabanından getir
    var reservations = await _context.Rezervasyonlar
        .AsNoTracking()
        .OrderByDescending(r => r.CreatedAt)
        .ThenByDescending(r => r.Id)
        .Take(5)
        .ToListAsync();

    // Bu rezervasyonların emlak ID'lerini al
    var emlakIds = reservations
        .Select(r => r.EmlakId)
        .Distinct()
        .ToList();

    // İlgili emlakları tek sorguda getir
    var emlaklar = await _context.Emlaklar
        .AsNoTracking()
        .Where(e => emlakIds.Contains(e.Id))
        .ToListAsync();

    var emlakDictionary = emlaklar.ToDictionary(
        e => e.Id,
        e => e.EmlakAdı
    );

    // DTO dönüşümünü C# tarafında yap
    var result = new List<DashboardRecentReservationResponse>();

    foreach (var r in reservations)
    {
        emlakDictionary.TryGetValue(
            r.EmlakId,
            out var emlakAdi
        );

        result.Add(new DashboardRecentReservationResponse
        {
            Id = r.Id,
            EmlakId = r.EmlakId,
            EmlakAdi = emlakAdi ?? "",
            MusteriAdi = r.MusteriAd + " " + r.MusteriSoyad,
            GirisTarihi = r.GirisTarihi,
            CikisTarihi = r.CikisTarihi,
            ToplamFiyat = r.ToplamFiyat,
            Durum = (int)r.Durum,
            CreatedAt = r.CreatedAt
        });
    }

    return result;
}



}
