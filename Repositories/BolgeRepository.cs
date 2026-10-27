using api_kiralama.Data;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class BolgeRepository
{
    private readonly AppDbContext _context;

    public BolgeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Bolge>> GetAllAsync()
    {
        return await _context.Bolgeler
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Bolge?> GetByIdAsync(int id)
    {
        return await _context.Bolgeler
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Bolge>> GetAltBolgelerAsync(int? parentId)
    {
        return await _context.Bolgeler
            .Where(b => b.ParentBolgeId == parentId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> HasAltBolgeAsync(int id)
    {
        return await _context.Bolgeler
            .AnyAsync(b => b.ParentBolgeId == id);
    }

    public async Task<bool> HasEmlakAsync(int id)
    {
        return await _context.Emlaklar
            .AnyAsync(e => e.BolgeId == id);
    }

    public async Task<Bolge> AddAsync(Bolge bolge)
    {
        await _context.Bolgeler.AddAsync(bolge);
        await _context.SaveChangesAsync();

        return bolge;
    }

    public async Task UpdateAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Bolge bolge)
    {
        _context.Bolgeler.Remove(bolge);
        await _context.SaveChangesAsync();
    }
}