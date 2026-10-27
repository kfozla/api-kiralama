using api_kiralama.Data;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class EmlakFiyatRepository
{
    private readonly AppDbContext _context;

    public EmlakFiyatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmlakFiyat>> GetByEmlakIdAsync(int emlakId)
    {
        return await _context.EmlakFiyatlari
            .Where(f => f.EmlakId == emlakId)
            .OrderBy(f => f.BaslangicTarihi)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<EmlakFiyat?> GetByIdAsync(int id)
    {
        return await _context.EmlakFiyatlari
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<bool> HasOverlapAsync(
        int emlakId,
        DateOnly baslangic,
        DateOnly bitis,
        int? excludeId = null)
    {
        return await _context.EmlakFiyatlari.AnyAsync(f =>
            f.EmlakId == emlakId &&
            (!excludeId.HasValue || f.Id != excludeId.Value) &&
            f.BaslangicTarihi < bitis &&
            f.BitisTarihi > baslangic);
    }

    public async Task<List<EmlakFiyat>> GetByDateRangeAsync(
        int emlakId, DateOnly baslangic, DateOnly bitis)
    {
        return await _context.EmlakFiyatlari
            .Where(f =>
                f.EmlakId == emlakId &&
                f.BaslangicTarihi < bitis &&
                f.BitisTarihi > baslangic)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<EmlakFiyat> AddAsync(EmlakFiyat fiyat)
    {
        await _context.EmlakFiyatlari.AddAsync(fiyat);
        await _context.SaveChangesAsync();
        return fiyat;
    }

    public async Task UpdateAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(EmlakFiyat fiyat)
    {
        _context.EmlakFiyatlari.Remove(fiyat);
        await _context.SaveChangesAsync();
    }
}