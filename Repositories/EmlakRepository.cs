using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class EmlakRepository
{
    private readonly AppDbContext _context;

    public EmlakRepository(AppDbContext context)
    {
        _context = context;
    }

    private IQueryable<Emlak> ActiveEmlaklar()
    {
        return _context.Emlaklar
            .Where(e => !e.IsDeleted);
    }

    public async Task<List<Emlak>> GetAllAsync()
    {
        return await ActiveEmlaklar()
            .Include(e => e.EmlakOzellikleri)
            .Include(e => e.OneCıkanOzellikler)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Emlak?> GetByIdAsync(int id)
    {
        return await ActiveEmlaklar()
            .Include(e => e.EmlakOzellikleri)
            .Include(e => e.OneCıkanOzellikler)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id);
    }
    public async Task<List<Emlak>> GetByEmlakSahibiIdAsync(int emlakSahibiId)
    {
        return await _context.Emlaklar
            .Where(e => e.EmlakSahibiId == emlakSahibiId && !e.IsDeleted)
            .Include(e => e.EmlakOzellikleri)
            .Include(e => e.OneCıkanOzellikler)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> SahipExistsAsync(int? id)
    {
        return await _context.EmlakSahipleri
            .AnyAsync(s => s.Id == id);
    }

    public async Task<bool> BolgeExistsAsync(int id)
    {
        return await _context.Bolgeler
            .AnyAsync(b => b.Id == id);
    }

    public async Task<List<EmlakOzellik>> GetOzelliklerAsync(
        List<int> ids)
    {
        return await _context.EmlakOzellikleri
            .Where(o => ids.Contains(o.Id))
            .ToListAsync();
    }

    public async Task<Emlak> AddAsync(Emlak emlak)
    {
        await _context.Emlaklar.AddAsync(emlak);
        await _context.SaveChangesAsync();

        return emlak;
    }

    public async Task UpdateAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Emlak emlak)
    {
        emlak.IsDeleted = true;
        emlak.DeletedAt = DateTime.UtcNow;
        emlak.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<List<Emlak>> FilterAsync(
        int? bolgeId,
        EmlakTipi? emlakTipi,
        int? minKapasite,
        List<int>? ozellikIds)
    {
        var query = ActiveEmlaklar();

        if (bolgeId.HasValue)
        {
            query = query.Where(e => e.BolgeId == bolgeId.Value);
        }

        if (emlakTipi.HasValue)
        {
            query = query.Where(e => e.EmlakTipi == emlakTipi.Value);
        }

        if (minKapasite.HasValue)
        {
            query = query.Where(
                e => e.Kapasite >= minKapasite.Value);
        }

        if (ozellikIds != null)
        {
            foreach (int id in ozellikIds.Distinct())
            {
                int featureId = id;

                query = query.Where(e =>
                    e.EmlakOzellikleri.Any(o => o.Id == featureId));
            }
        }

        return await query
            .Include(e => e.EmlakOzellikleri)
            .Include(e => e.OneCıkanOzellikler)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }
    
    
    public async Task<PagedResponse<EmlakListResponse>> GetPagedAsync(
        EmlakPagedRequest request)
    {
        int page = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, 100);

        IQueryable<Emlak> query = _context.Emlaklar
            .AsNoTracking()
            .Where(e => !e.IsDeleted);

        // Emlak adına göre arama
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(e =>
                EF.Functions.Like(e.EmlakAdı, $"%{search}%"));
        }

        // Bölge filtresi
        if (request.BolgeIds?.Count > 0)
        {
            var bolgeIds = request.BolgeIds
                .Distinct()
                .ToList();

            query = query.Where(e =>
                bolgeIds.Contains(e.BolgeId));
        }

        // Emlak tipi filtresi
        if (request.EmlakTipi.HasValue)
        {
            query = query.Where(e =>
                (int)e.EmlakTipi == request.EmlakTipi.Value);
        }

        // Minimum kapasite
        if (request.MinKapasite.HasValue)
        {
            query = query.Where(e =>
                e.Kapasite >= request.MinKapasite.Value);
        }

        // Özellik filtresi
        if (request.OzellikIds?.Count > 0)
        {
            var ozellikIds = request.OzellikIds
                .Distinct()
                .ToList();

            foreach (var ozellikId in ozellikIds)
            {
                query = query.Where(e =>
                    e.EmlakOzellikleri.Any(o => o.Id == ozellikId));
            }
        }

        // Pagination öncesi toplam eşleşen kayıt sayısı
        
        int totalCount = await query.CountAsync();

// 1. Yalnızca mevcut sayfanın emlaklarını getir
        var emlaklar = await query
            .OrderByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

// 2. Bu sayfadaki emlak ID'lerini al
        var emlakIds = emlaklar
            .Select(e => e.Id)
            .ToList();

// 3. Bu emlakların kapak fotoğrafı yollarını getir
        var kapaklar = await _context.EmlakFotograflari
            .AsNoTracking()
            .Where(f =>
                emlakIds.Contains(f.EmlakId) &&
                f.KapakMi)
            .ToListAsync();

// 4. EmlakId -> Kapak fotoğrafı yolu
        var kapakMap = new Dictionary<int, string>();

        foreach (var fotograf in kapaklar)
        {
            kapakMap[fotograf.EmlakId] = fotograf.DosyaYolu;
        }

// 5. Liste DTO'suna dönüştür
        var items = new List<EmlakListResponse>();

        foreach (var emlak in emlaklar)
        {
            kapakMap.TryGetValue(
                emlak.Id,
                out var kapakUrl
            );

            items.Add(new EmlakListResponse
            {
                Id = emlak.Id,
                EmlakAdı = emlak.EmlakAdı,
                BolgeId = emlak.BolgeId,
                EmlakTipi = (int)emlak.EmlakTipi,
                Kapasite = emlak.Kapasite,
                YatakOdasıSayısı = emlak.YatakOdasıSayısı,
                BanyoSayısı = emlak.BanyoSayısı,
                KapakFotografUrl = kapakUrl,
                Yayinda = emlak.Yayinda,
                OtoparkSayisi = emlak.OtoparkSayisi,
                BelgeNo = emlak.BelgeNo,
            });
        }

// 6. Pagination response
        return new PagedResponse<EmlakListResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

    }
    
    public async Task<List<EmlakSearchResponse>> SearchAsync(
        string query,
        int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) ||
            query.Trim().Length < 2)
        {
            return new();
        }

        query = query.Trim();
        limit = Math.Clamp(limit, 1, 20);

        return await _context.Emlaklar
            .AsNoTracking()
            .Where(e =>
                !e.IsDeleted &&
                EF.Functions.Like(
                    e.EmlakAdı,
                    $"%{query}%"
                )
            )
            .OrderBy(e => e.EmlakAdı)
            .ThenBy(e => e.Id)
            .Take(limit)
            .Select(e => new EmlakSearchResponse
            {
                Id = e.Id,
                EmlakAdi = e.EmlakAdı
            })
            .ToListAsync();
    }
    public async Task<bool> UpdateYayinDurumuAsync(int id, bool yayinda)
    {
        var emlak = await _context.Emlaklar
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (emlak == null)
            return false;

        emlak.Yayinda = yayinda;

        await _context.SaveChangesAsync();

        return true;
    }


}