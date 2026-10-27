using api_kiralama.Data;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class EmlakFotografRepository
{
    private readonly AppDbContext _context;

    public EmlakFotografRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> EmlakExistsAsync(int emlakId)
    {
        return _context.Emlaklar.AnyAsync(
            e => e.Id == emlakId && !e.IsDeleted
        );
    }

    public Task<List<EmlakFotograf>> GetByEmlakIdAsync(
        int emlakId)
    {
        return _context.EmlakFotograflari
            .AsNoTracking()
            .Where(f => f.EmlakId == emlakId)
            .OrderBy(f => f.Sira)
            .ThenBy(f => f.Id)
            .ToListAsync();
    }

    // Güncelleme ve silme işlemleri için tracked kayıtlar.
    public Task<List<EmlakFotograf>> GetTrackedAsync(
        int emlakId)
    {
        return _context.EmlakFotograflari
            .Where(f => f.EmlakId == emlakId)
            .OrderBy(f => f.Sira)
            .ThenBy(f => f.Id)
            .ToListAsync();
    }

    public Task<EmlakFotograf?> GetByIdAsync(
        int emlakId,
        int fotografId)
    {
        return _context.EmlakFotograflari
            .FirstOrDefaultAsync(f =>
                f.Id == fotografId &&
                f.EmlakId == emlakId
            );
    }

    public void AddRange(IEnumerable<EmlakFotograf> fotograflar)
    {
        _context.EmlakFotograflari.AddRange(fotograflar);
    }

    public void Delete(EmlakFotograf fotograf)
    {
        _context.EmlakFotograflari.Remove(fotograf);
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}