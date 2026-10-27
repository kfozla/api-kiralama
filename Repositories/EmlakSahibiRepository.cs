using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class EmlakSahibiRepository
{
    private readonly AppDbContext _context;

    public EmlakSahibiRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmlakSahibi>> GetAllAsync()
    {
        return await _context.EmlakSahipleri
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<EmlakSahibiSearchResponse>> SearchAsync(
        string query,
        int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<EmlakSahibiSearchResponse>();
        }

        query = query.Trim();

        if (query.Length < 2)
        {
            return new List<EmlakSahibiSearchResponse>();
        }

        limit = Math.Clamp(limit, 1, 20);

        // Kullanıcının yazdığı SQL LIKE özel karakterlerini
        // normal metin olarak ele alıyoruz.
        var escapedQuery = query
            .Replace("~", "~~")
            .Replace("%", "~%")
            .Replace("_", "~_")
            .Replace("[", "~[");

        var pattern = $"%{escapedQuery}%";

        return await _context.EmlakSahipleri
            .AsNoTracking()
            .Where(x =>
                EF.Functions.Like(
                    x.Ad + " " + x.Soyad,
                    pattern,
                    "~"
                ) ||
                EF.Functions.Like(x.Ad, pattern, "~") ||
                EF.Functions.Like(x.Soyad, pattern, "~")
            )
            .OrderBy(x => x.Ad)
            .ThenBy(x => x.Soyad)
            .ThenBy(x => x.Id)
            .Take(limit)
            .Select(x => new EmlakSahibiSearchResponse
            {
                Id = x.Id,
                Ad = x.Ad,
                Soyad = x.Soyad
            })
            .ToListAsync();
    }


    public async Task<EmlakSahibi?> GetByIdAsync(int id)
    {
        return await _context.EmlakSahipleri
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<EmlakSahibi> AddAsync(EmlakSahibi emlakSahibi)
    {
        await _context.EmlakSahipleri.AddAsync(emlakSahibi);
        await _context.SaveChangesAsync();

        return emlakSahibi;
    }

    public async Task UpdateAsync(EmlakSahibi emlakSahibi)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(EmlakSahibi emlakSahibi)
    {
        _context.EmlakSahipleri.Remove(emlakSahibi);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasEmlakAsync(int emlakSahibiId)
    {
        return await _context.Emlaklar
            .AnyAsync(e => e.EmlakSahibiId == emlakSahibiId);
    }
    
    
    public async Task<PagedResponse<EmlakSahibiListResponse>> GetPagedAsync(
        EmlakSahibiPagedRequest request)
    {
        int page = Math.Max(1, request.Page);
        int pageSize = Math.Clamp(request.PageSize, 1, 100);

        IQueryable<EmlakSahibi> query = _context.EmlakSahipleri
            .AsNoTracking();

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                EF.Functions.Like(x.Ad, $"%{search}%") ||
                EF.Functions.Like(x.Soyad, $"%{search}%") ||
                EF.Functions.Like(
                    x.Ad + " " + x.Soyad,
                    $"%{search}%"
                ) ||
                EF.Functions.Like(x.Telefon, $"%{search}%")
            );
        }

        // Filtrelenmiş toplam kayıt
        int totalCount = await query.CountAsync();

        // Yalnızca istenen sayfanın kayıtlarını getir
        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            
            .Select(x => new EmlakSahibiListResponse
            {
                Id = x.Id,
                Ad = x.Ad,
                Soyad = x.Soyad,
                Telefon = x.Telefon,
                Email = x.Email,
                Aciklama = x.Aciklama,
                KayitTarihi = x.KayitTarihi,

                EmlakSayisi = _context.Emlaklar.Count(e =>
                    e.EmlakSahibiId == x.Id &&
                    !e.IsDeleted
                )
            })
            .ToListAsync();

        return new PagedResponse<EmlakSahibiListResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

}