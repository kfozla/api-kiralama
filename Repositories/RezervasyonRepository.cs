using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class RezervasyonRepository
{
    private readonly AppDbContext _context;

    public RezervasyonRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rezervasyon>> GetAllAsync()
    {
        return await _context.Rezervasyonlar
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Rezervasyon?> GetByIdAsync(int id)
    {
        return await _context.Rezervasyonlar
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Rezervasyon>> GetByEmlakIdAsync(int emlakId)
    {
        return await _context.Rezervasyonlar
            .Where(r => r.EmlakId == emlakId)
            .OrderByDescending(r => r.GirisTarihi)
            .AsNoTracking()
            .ToListAsync();
    }

    
    public async Task<bool> HasConflictAsync(
        int emlakId,
        DateOnly giris,
        DateOnly cikis,
        int? excludeReservationId = null)
    {
        return await _context.EmlakBlokeTarihler
            .AnyAsync(b =>
                b.EmlakId == emlakId &&
                (!excludeReservationId.HasValue ||
                 b.RezervasyonId != excludeReservationId.Value) &&
                b.BaslangicTarihi < cikis &&
                b.BitisTarihi > giris);
    }


    public async Task<Rezervasyon> AddAsync(Rezervasyon rezervasyon)
    {
        await _context.Rezervasyonlar.AddAsync(rezervasyon);
        await _context.SaveChangesAsync();

        return rezervasyon;
    }

    public async Task UpdateAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<Rezervasyon>> GetByDateRangeAsync(
        int emlakId,
        DateOnly baslangic,
        DateOnly bitis)
    {
        return await _context.Rezervasyonlar
            .Where(r =>
                r.EmlakId == emlakId &&
                r.Durum != RezervasyonDurumu.IptalEdildi &&
                r.GirisTarihi < bitis &&
                r.CikisTarihi > baslangic)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task DeleteAsync(Rezervasyon rezervasyon)
    {
        _context.Rezervasyonlar.Remove(rezervasyon);
        await _context.SaveChangesAsync();
    }
    
    
public async Task<PagedResponse<RezervasyonListResponse>> GetPagedAsync(
    RezervasyonPagedRequest request)
{
    int page = Math.Max(1, request.Page);
    int pageSize = Math.Clamp(request.PageSize, 1, 100);

    // Rezervasyonları emlaklarla eşleştir.
    // Navigation property kullanmıyoruz.
    var query =
        from r in _context.Rezervasyonlar.AsNoTracking()
        join e in _context.Emlaklar.AsNoTracking()
            on r.EmlakId equals e.Id into emlakGroup
        from e in emlakGroup.DefaultIfEmpty()
        select new
        {
            Rezervasyon = r,
            EmlakAdi = e != null ? e.EmlakAdı : ""
        };

    // Genel arama:
    // Rezervasyon no, müşteri adı, soyadı ve emlak adı.
    if (!string.IsNullOrWhiteSpace(request.Search))
    {
        string search = request.Search.Trim();

        bool isNumber = int.TryParse(
            search,
            out int rezervasyonId
        );

        query = query.Where(x =>
            (isNumber && x.Rezervasyon.Id == rezervasyonId) ||

            EF.Functions.Like(
                x.Rezervasyon.MusteriAd,
                $"%{search}%"
            ) ||

            EF.Functions.Like(
                x.Rezervasyon.MusteriSoyad,
                $"%{search}%"
            ) ||

            EF.Functions.Like(
                x.Rezervasyon.MusteriAd + " " +
                x.Rezervasyon.MusteriSoyad,
                $"%{search}%"
            ) ||

            EF.Functions.Like(
                x.EmlakAdi,
                $"%{search}%"
            )
        );
    }

    // Rezervasyon durumu
    if (request.Durum.HasValue)
    {
        query = query.Where(x =>
            (int)x.Rezervasyon.Durum == request.Durum.Value
        );
    }

    // Giriş tarihi başlangıcı
    if (request.GirisBaslangic.HasValue)
    {
        query = query.Where(x =>
            x.Rezervasyon.GirisTarihi >=
            request.GirisBaslangic.Value
        );
    }

    // Giriş tarihi bitişi
    if (request.GirisBitis.HasValue)
    {
        query = query.Where(x =>
            x.Rezervasyon.GirisTarihi <=
            request.GirisBitis.Value
        );
    }

    // Tüm filtreler uygulandıktan sonraki kayıt sayısı
    int totalCount = await query.CountAsync();

    // En son oluşturulan rezervasyon en üstte.
    var items = await query
        .OrderByDescending(x => x.Rezervasyon.CreatedAt)
        .ThenByDescending(x => x.Rezervasyon.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(x => new RezervasyonListResponse
        {
            Id = x.Rezervasyon.Id,

            EmlakId = x.Rezervasyon.EmlakId,
            EmlakAdi = x.EmlakAdi,

            MusteriAd = x.Rezervasyon.MusteriAd,
            MusteriSoyad = x.Rezervasyon.MusteriSoyad,

            Telefon = x.Rezervasyon.Telefon,
            Email = x.Rezervasyon.Email,

            GirisTarihi = x.Rezervasyon.GirisTarihi,
            CikisTarihi = x.Rezervasyon.CikisTarihi,

            KisiSayisi = x.Rezervasyon.KisiSayisi,
            ToplamFiyat = x.Rezervasyon.ToplamFiyat,
            
            TemizlikUcreti = x.Rezervasyon.TemizlikUcreti,
            HasarDepozitosu = x.Rezervasyon.HasarDepozitosu,

            Durum = (int)x.Rezervasyon.Durum,

            CreatedAt = x.Rezervasyon.CreatedAt,
            UpdatedAt = x.Rezervasyon.UpdatedAt
        })
        .ToListAsync();

    return new PagedResponse<RezervasyonListResponse>
    {
        Items = items,
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount
    };
}

public async Task<RezervasyonStatsResponse> GetStatsAsync()
{
    var stats = await _context.Rezervasyonlar
        .AsNoTracking()
        .GroupBy(r => r.Durum)
        .Select(g => new
        {
            Durum = (int)g.Key,
            Adet = g.Count(),
            ToplamTutar = g.Sum(r => r.ToplamFiyat)
        })
        .ToListAsync();

    var result = new RezervasyonStatsResponse
    {
        ToplamRezervasyon = stats.Sum(x => x.Adet),

        Bekleyen = stats
            .Where(x => x.Durum == 0)
            .Sum(x => x.Adet),

        Onaylanan = stats
            .Where(x => x.Durum == 1)
            .Sum(x => x.Adet),

        IptalEdilen = stats
            .Where(x => x.Durum == 2)
            .Sum(x => x.Adet),

        Tamamlanan = stats
            .Where(x => x.Durum == 3)
            .Sum(x => x.Adet),

        ToplamGelir = stats
            .Where(x => x.Durum != 2)
            .Sum(x => x.ToplamTutar)
    };

    return result;
}


public async Task<RezervasyonTakvimResponse> GetTakvimAsync(
    RezervasyonTakvimRequest request)
{
    var start = new DateOnly(
        request.Year,
        request.Month,
        1
    );

    var end = start.AddMonths(1);

    // Ortak sorgu: seçili villa veya tüm villalar
    var baseQuery = _context.Rezervasyonlar
        .AsNoTracking()
        .AsQueryable();

    if (request.EmlakId.HasValue)
    {
        baseQuery = baseQuery.Where(r =>
            r.EmlakId == request.EmlakId.Value
        );
    }

    // İptal edilenler takvimde görünmeyecek.
    var monthQuery = baseQuery.Where(r =>
        r.Durum != RezervasyonDurumu.IptalEdildi
    );

    if (!request.ShowPending)
    {
        monthQuery = monthQuery.Where(r =>
            r.Durum != RezervasyonDurumu.Beklemede
        );
    }

    // GİRİŞ veya ÇIKIŞ seçili ayın içindeyse getir.
    // Çıkış sonraki aya taşarsa o ayda da görünür.
    monthQuery = monthQuery.Where(r =>
        (r.GirisTarihi >= start &&
         r.GirisTarihi < end) ||

        (r.CikisTarihi >= start &&
         r.CikisTarihi < end)
    );

    var reservations = await (
        from r in monthQuery
        join e in _context.Emlaklar.AsNoTracking()
            on r.EmlakId equals e.Id into emlakGroup
        from e in emlakGroup.DefaultIfEmpty()
        orderby r.GirisTarihi, r.Id
        select new RezervasyonTakvimItemResponse
        {
            Id = r.Id,
            EmlakId = r.EmlakId,

            EmlakAdi = e != null
                ? e.EmlakAdı
                : "",

            MusteriAd = r.MusteriAd,
            MusteriSoyad = r.MusteriSoyad,

            Telefon = r.Telefon,
            Email = r.Email,

            GirisTarihi = r.GirisTarihi,
            CikisTarihi = r.CikisTarihi,

            KisiSayisi = r.KisiSayisi,
            Durum = (int)r.Durum
        }
    ).ToListAsync();

    // Bugünkü giriş / çıkış:
    // Yalnızca onaylı ve tamamlanan kayıtlar.
    var operational = baseQuery.Where(r =>
        r.Durum == RezervasyonDurumu.Onaylandi ||
        r.Durum == RezervasyonDurumu.Tamamlandi
    );

    int todayEntries = await operational.CountAsync(r =>
        r.GirisTarihi == request.Today
    );

    int todayExits = await operational.CountAsync(r =>
        r.CikisTarihi == request.Today
    );

    // Bekleyen toplam talepler seçili aydan bağımsız.
    int pendingCount = await baseQuery.CountAsync(r =>
        r.Durum == RezervasyonDurumu.Beklemede
    );

    return new RezervasyonTakvimResponse
    {
        Reservations = reservations,
        TodayEntries = todayEntries,
        TodayExits = todayExits,
        PendingCount = pendingCount
    };
}



}