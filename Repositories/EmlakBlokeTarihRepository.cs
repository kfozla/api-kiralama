using api_kiralama.Data;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class EmlakBlokeTarihRepository
{
    private readonly AppDbContext _context;

    public EmlakBlokeTarihRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmlakBlokeTarih>> GetByEmlakIdAsync(
        int emlakId)
    {
        return await _context.EmlakBlokeTarihler
            .Where(x => x.EmlakId == emlakId)
            .OrderBy(x => x.BaslangicTarihi)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<EmlakBlokeTarih?> GetByIdAsync(int id)
    {
        return await _context.EmlakBlokeTarihler
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> HasOverlapAsync(
        int emlakId,
        DateOnly baslangic,
        DateOnly bitis,
        int? excludeId = null)
    {
        return await _context.EmlakBlokeTarihler
            .AnyAsync(b =>
                b.EmlakId == emlakId &&
                (!excludeId.HasValue || b.Id != excludeId.Value) &&
                b.BaslangicTarihi < bitis &&
                b.BitisTarihi > baslangic);
    }

    public async Task<EmlakBlokeTarih> AddAsync(
        EmlakBlokeTarih bloke)
    {
        await _context.EmlakBlokeTarihler.AddAsync(bloke);
        await _context.SaveChangesAsync();
        return bloke;
    }
    public async Task UpdateAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(EmlakBlokeTarih bloke)
    {
        _context.EmlakBlokeTarihler.Remove(bloke);
        await _context.SaveChangesAsync();
    }
}