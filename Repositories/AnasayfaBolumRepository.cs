
using api_kiralama.Data;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class AnasayfaBolumRepository
{
    private readonly AppDbContext _context;

    public AnasayfaBolumRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AnasayfaBolum>> GetAllAsync()
    {
        return await _context.AnasayfaBolumler
            .AsNoTracking()
            .Include(x => x.Emlaklar)
            .OrderBy(x => x.Sira)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<AnasayfaBolum?> GetByIdAsync(int id)
    {
        return await _context.AnasayfaBolumler
            .Include(x => x.Emlaklar)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<AnasayfaBolum>> GetActiveAsync()
    {
        return await _context.AnasayfaBolumler
            .AsNoTracking()
            .Include(x => x.Emlaklar)
            .Where(x => x.Aktif)
            .OrderBy(x => x.Sira)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<bool> SlugExistsAsync(
        string slug,
        int? excludeId = null)
    {
        return await _context.AnasayfaBolumler
            .AnyAsync(x =>
                x.Slug == slug &&
                (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    public async Task<bool> EmlaklarExistAsync(
        IEnumerable<int> emlakIds)
    {
        var ids = emlakIds.Distinct().ToList();

        var count = await _context.Emlaklar
            .CountAsync(x => ids.Contains(x.Id) && !x.IsDeleted && x.Yayinda);

        return count == ids.Count;
    }

    public async Task<List<Emlak>> GetLatestEmlaklarAsync(int limit)
    {
        return await _context.Emlaklar
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Yayinda)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Emlak>> GetEmlaklarByIdsAsync(
        IEnumerable<int> emlakIds)
    {
        var ids = emlakIds.Distinct().ToList();

        return await _context.Emlaklar
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted &&!x.Yayinda)
            .ToListAsync();
    }

    public async Task AddAsync(AnasayfaBolum bolum)
    {
        await _context.AnasayfaBolumler.AddAsync(bolum);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AnasayfaBolum bolum)
    {
        _context.AnasayfaBolumler.Remove(bolum);
        await _context.SaveChangesAsync();
    }

    public void RemoveBolumEmlaklar(
        IEnumerable<AnasayfaBolumEmlak> emlaklar)
    {
        _context.AnasayfaBolumEmlaklar.RemoveRange(emlaklar);
    }

    public async Task AddBolumEmlaklarAsync(
        IEnumerable<AnasayfaBolumEmlak> emlaklar)
    {
        await _context.AnasayfaBolumEmlaklar.AddRangeAsync(emlaklar);
    }
}
