
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class EmlakPaylasimRepository
{
    private readonly AppDbContext _context;

    public EmlakPaylasimRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<int>> GetYayindakiEmlakIdsAsync(
        List<int> ids)
    {
        return await _context.Emlaklar
            .AsNoTracking()
            .Where(e =>
                ids.Contains(e.Id) &&
                !e.IsDeleted &&
                e.Yayinda)
            .Select(e => e.Id)
            .ToListAsync();
    }

    public async Task AddAsync(EmlakPaylasim paylasim)
    {
        await _context.EmlakPaylasimlar.AddAsync(paylasim);
        await _context.SaveChangesAsync();
    }

    public async Task<EmlakPaylasim?> GetByTokenHashAsync(
        string tokenHash)
    {
        return await _context.EmlakPaylasimlar
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.TokenHash == tokenHash &&
                !p.IsRevoked &&
                p.ExpiresAt > DateTime.UtcNow);
    }

    
    public async Task<List<EmlakPaylasimEmlakResponse>>
        GetPaylasilanEmlaklarAsync(int paylasimId)
    {
        return await (
            from item in _context.EmlakPaylasimItems.AsNoTracking()

            join emlak in _context.Emlaklar.AsNoTracking()
                on item.EmlakId equals emlak.Id

            where item.EmlakPaylasimId == paylasimId
                  && !emlak.IsDeleted
                  && emlak.Yayinda

            orderby item.Sira

            select new EmlakPaylasimEmlakResponse
            {
                Id = emlak.Id,
                EmlakAdi = emlak.EmlakAdı,
                BolgeId = emlak.BolgeId,
                Kapasite = emlak.Kapasite,
                YatakOdasiSayisi = emlak.YatakOdasıSayısı,
                BanyoSayisi = emlak.BanyoSayısı,

                KapakFotografUrl = _context.EmlakFotograflari
                    .Where(f =>
                        f.EmlakId == emlak.Id &&
                        f.KapakMi)
                    .Select(f => f.DosyaYolu)
                    .FirstOrDefault()
            }
        ).ToListAsync();
    }

}
