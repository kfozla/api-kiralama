using api_kiralama.Data;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class IsletmeAyarlariRepository
{
    private readonly AppDbContext _context;

    public IsletmeAyarlariRepository(AppDbContext context)
    {
        _context = context;
    }

    
    public async Task<IsletmeAyarlari> GetOrCreateAsync()
    {
        var ayarlar = await _context.IsletmeAyarlari
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (ayarlar != null)
            return ayarlar;

        ayarlar = new IsletmeAyarlari
        {
            IsletmeAdi = "Modern Property",
            TursabUyeligiVar = false,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.IsletmeAyarlari.AddAsync(ayarlar);
        await _context.SaveChangesAsync();

        return ayarlar;
    }


    public Task SaveAsync()
    {
        return _context.SaveChangesAsync();
    }
}