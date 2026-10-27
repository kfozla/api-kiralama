using api_kiralama.Data;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class EmlakOzellikRepository
{
    private readonly AppDbContext _context;

    public EmlakOzellikRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmlakOzellik>> GetAllAsync()
    {
        return await _context.EmlakOzellikleri
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<EmlakOzellik?> GetByIdAsync(int id)
    {
        return await _context.EmlakOzellikleri
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    
    public async Task<EmlakOzellik?> GetByIdForLogoAsync(int id)
    {
        return await _context.EmlakOzellikleri
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<EmlakOzellik>> GetByKategoriAsync(
        OzellikKategori kategori)
    {
        return await _context.EmlakOzellikleri
            .Where(x => x.Kategori == kategori)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<EmlakOzellik>> GetByIdsAsync(
        List<int> ids)
    {
        return await _context.EmlakOzellikleri
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(
        string ozellikAdi,
        OzellikKategori kategori,
        int? excludeId = null)
    {
        return await _context.EmlakOzellikleri
            .AnyAsync(x =>
                x.OzellikAdi == ozellikAdi &&
                x.Kategori == kategori &&
                (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    public async Task<EmlakOzellik> AddAsync(EmlakOzellik ozellik)
    {
        await _context.EmlakOzellikleri.AddAsync(ozellik);
        await _context.SaveChangesAsync();

        return ozellik;
    }

    public async Task UpdateAsync(EmlakOzellik ozellik)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(EmlakOzellik ozellik)
    {
        _context.EmlakOzellikleri.Remove(ozellik);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsUsedAsync(int ozellikId)
    {
        return await _context.Emlaklar
            .AnyAsync(e =>
                e.EmlakOzellikleri.Any(o => o.Id == ozellikId));
    }
}