using api_kiralama.Data;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Repositories;

public class EmlakTakvimRepository
{
    private readonly AppDbContext _context;

    public EmlakTakvimRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EmlakExistsAsync(int emlakId)
    {
        return await _context.Emlaklar
            .AnyAsync(e => e.Id == emlakId && !e.IsDeleted);
    }

    public async Task<List<EmlakFiyat>> GetFiyatlarAsync(
        int emlakId,
        DateOnly baslangic,
        DateOnly bitis)
    {
        return await _context.EmlakFiyatlari
            .Where(f =>
                f.EmlakId == emlakId &&
                f.BaslangicTarihi < bitis &&
                f.BitisTarihi > baslangic)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Rezervasyon>> GetRezervasyonlarAsync(
        int emlakId,
        DateOnly baslangic,
        DateOnly bitis)
    {
        return await _context.Rezervasyonlar
            .Where(r =>
                r.EmlakId == emlakId &&
                r.Durum != RezervasyonDurumu.IptalEdildi &&
                r.GirisTarihi < bitis &&
                r.CikisTarihi > baslangic)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<EmlakBlokeTarih>> GetBlokelerAsync(
        int emlakId,
        DateOnly baslangic,
        DateOnly bitis)
    {
        return await _context.EmlakBlokeTarihler
            .Where(b =>
                b.EmlakId == emlakId &&
                b.BaslangicTarihi < bitis &&
                b.BitisTarihi > baslangic)
            .AsNoTracking()
            .ToListAsync();
    }
}