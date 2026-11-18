
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Services;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class MuhasebeRepository
{
    private readonly AppDbContext _context;

    public MuhasebeRepository(AppDbContext context, EmlakService emlakService)
    {
        _context = context;
    }

    public async Task<(List<MuhasebeRezervasyonResponse> Items, int TotalCount)>
        GetPagedAsync(MuhasebePagedRequest request)
    {
        var query = _context.Rezervasyonlar
            .AsNoTracking()
            .Where(r =>
                r.Durum == RezervasyonDurumu.Onaylandi);

        if (request.EmlakId.HasValue)
        {
            query = query.Where(r =>
                r.EmlakId == request.EmlakId.Value);
        }

        if (request.GirisTarihi.HasValue)
        {
            query = query.Where(r =>
                r.GirisTarihi >= request.GirisTarihi.Value);
        }

        if (request.CikisTarihi.HasValue)
        {
            query = query.Where(r =>
                r.GirisTarihi <= request.CikisTarihi.Value);
        }
        

        var totalCount = await query.CountAsync();

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new MuhasebeRezervasyonResponse
            {
                Id = r.Id,
                EmlakId = r.EmlakId,
                EmlakAdi = _context.Emlaklar
                    .Where(e => e.Id == r.EmlakId)
                    .Select(e => e.EmlakAdı)
                    .FirstOrDefault() ?? "",

                MusteriAd = r.MusteriAd,
                MusteriSoyad = r.MusteriSoyad,

                GirisTarihi = r.GirisTarihi,
                CikisTarihi = r.CikisTarihi,

                ToplamFiyat = r.ToplamFiyat,
                TemizlikUcreti = r.TemizlikUcreti,
                OnOdemeTutari = r.OnOdemeTutari,
                KomisyonTutari = r.KomisyonTutari,

                GiristeOdenecek =
                    r.ToplamFiyat - r.OnOdemeTutari,

                EmlakSahibiHakedis =
                    r.ToplamFiyat+r.TemizlikUcreti - r.KomisyonTutari,

                AcenteninBorcu =
                    r.OnOdemeTutari > r.KomisyonTutari
                        ? r.OnOdemeTutari - r.KomisyonTutari
                        : 0m,

                KomisyonAlacagi =
                    r.KomisyonTutari > r.OnOdemeTutari
                        ? r.KomisyonTutari - r.OnOdemeTutari
                        : 0m
            })
            .ToListAsync();

        return (items, totalCount);
    }
    
    public async Task<MuhasebeOzetResponse> GetOzetAsync(
        MuhasebeOzetRequest request)
    {
        var query = _context.Rezervasyonlar
            .AsNoTracking()
            .Where(r =>
                r.Durum == RezervasyonDurumu.Onaylandi);

        // Giriş tarihi başlangıç filtresi
        if (request.BaslangicTarihi.HasValue)
        {
            query = query.Where(r =>
                r.GirisTarihi >= request.BaslangicTarihi.Value);
        }

        // Giriş tarihi bitiş filtresi (dahil)
        if (request.BitisTarihi.HasValue)
        {
            query = query.Where(r =>
                r.GirisTarihi <= request.BitisTarihi.Value);
        }

        // Emlak filtresi
        if (request.EmlakId.HasValue)
        {
            query = query.Where(r =>
                r.EmlakId == request.EmlakId.Value);
        }

        // Hesaplamaların tamamı SQL tarafında yapılır.
        var ozet = await query
            .GroupBy(r => 1)
            .Select(g => new MuhasebeOzetResponse
            {
                RezervasyonSayisi = g.Count(),

                ToplamRezervasyonHacmi =
                    g.Sum(r => r.ToplamFiyat+r.TemizlikUcreti),

                ToplamOnOdeme =
                    g.Sum(r => r.OnOdemeTutari),

                ToplamKomisyon =
                    g.Sum(r => r.KomisyonTutari),

                ToplamEmlakSahibiHakedis =
                    g.Sum(r =>
                        r.ToplamFiyat+r.TemizlikUcreti - r.KomisyonTutari),

                ToplamGiristeOdenecek =
                    g.Sum(r =>
                        r.ToplamFiyat+r.TemizlikUcreti - r.OnOdemeTutari),

                ToplamAcenteBorcu =
                    g.Sum(r =>
                        r.OnOdemeTutari > r.KomisyonTutari
                            ? r.OnOdemeTutari - r.KomisyonTutari
                            : 0m),

                ToplamKomisyonAlacagi =
                    g.Sum(r =>
                        r.KomisyonTutari > r.OnOdemeTutari
                            ? r.KomisyonTutari - r.OnOdemeTutari
                            : 0m)
            })
            .FirstOrDefaultAsync();

        return ozet ?? new MuhasebeOzetResponse();
    }
    public async Task<List<MuhasebeAylikGrafikResponse>> GetAylikGrafikAsync(
        MuhasebeOzetRequest request)
    {
        var query = _context.Rezervasyonlar
            .AsNoTracking()
            .Where(r => r.Durum == RezervasyonDurumu.Onaylandi);

        if (request.BaslangicTarihi.HasValue)
            query = query.Where(r =>
                r.GirisTarihi >= request.BaslangicTarihi.Value);

        if (request.BitisTarihi.HasValue)
            query = query.Where(r =>
                r.GirisTarihi <= request.BitisTarihi.Value);

        if (request.EmlakId.HasValue)
            query = query.Where(r => r.EmlakId == request.EmlakId.Value);

        // Hesaplamalar SQL tarafında yapılır; tüm kayıtlar belleğe çekilmez.
        return await query
            .GroupBy(r => new { Yil = r.GirisTarihi.Year, Ay = r.GirisTarihi.Month })
            .Select(g => new MuhasebeAylikGrafikResponse
            {
                Yil = g.Key.Yil,
                Ay = g.Key.Ay,
                RezervasyonHacmi = g.Sum(r => r.ToplamFiyat+r.TemizlikUcreti),
                OnOdeme = g.Sum(r => r.OnOdemeTutari),
                Komisyon = g.Sum(r => r.KomisyonTutari),
                EvSahibiHakedis = g.Sum(r => r.ToplamFiyat+r.TemizlikUcreti - r.KomisyonTutari)
            })
            .OrderBy(x => x.Yil)
            .ThenBy(x => x.Ay)
            .ToListAsync();
    }

}
