
using System.Data;
using api_kiralama.Data;
using api_kiralama.Models;
using api_kiralama.Repositories;
using api_kiralama.Dtos;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class EmlakBlokeTarihService
{
    private readonly AppDbContext _context;
    private readonly EmlakBlokeTarihRepository _repository;
    private readonly EmlakTakvimLock _takvimLock;

    public EmlakBlokeTarihService(
        AppDbContext context,
        EmlakBlokeTarihRepository repository,
        EmlakTakvimLock takvimLock)
    {
        _context = context;
        _repository = repository;
        _takvimLock = takvimLock;
    }

    public Task<List<EmlakBlokeTarih>> GetByEmlakIdAsync(
        int emlakId)
        => _repository.GetByEmlakIdAsync(emlakId);

    private static void ValidateDates(
        DateOnly baslangic,
        DateOnly bitis)
    {
        if (baslangic >= bitis)
        {
            throw new ArgumentException(
                "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        }
    }

    // Yalnızca bloke tablosunu kontrol eder.
    // Bitiş tarihi hariçtir.
    private async Task<bool> HasOverlapAsync(
        int emlakId,
        DateOnly baslangic,
        DateOnly bitis,
        int? excludeBlockId = null)
    {
        return await _context.EmlakBlokeTarihler
            .AnyAsync(b =>
                b.EmlakId == emlakId &&
                (!excludeBlockId.HasValue ||
                 b.Id != excludeBlockId.Value) &&
                b.BaslangicTarihi < bitis &&
                b.BitisTarihi > baslangic);
    }

    // MANUEL BLOKE EKLE
    public async Task<EmlakBlokeTarih> AddAsync(
        EmlakBlokeTarihRequest request)
    {
        ValidateDates(
            request.BaslangicTarihi,
            request.BitisTarihi);

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        await _takvimLock.LockAsync(request.EmlakId);

        bool emlakVar = await _context.Emlaklar
            .AnyAsync(e =>
                e.Id == request.EmlakId &&
                !e.IsDeleted);

        if (!emlakVar)
        {
            throw new ArgumentException("Emlak bulunamadı.");
        }

        bool conflict = await HasOverlapAsync(
            request.EmlakId,
            request.BaslangicTarihi,
            request.BitisTarihi);

        if (conflict)
        {
            throw new InvalidOperationException(
                "Bu tarih aralığında zaten bloke bulunuyor.");
        }

        var bloke = new EmlakBlokeTarih
        {
            EmlakId = request.EmlakId,

            // Manuel bloke olduğu için null.
            RezervasyonId = null,

            BaslangicTarihi = request.BaslangicTarihi,
            BitisTarihi = request.BitisTarihi,
            Aciklama = request.Aciklama
        };

        _context.EmlakBlokeTarihler.Add(bloke);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return bloke;
    }

    // MANUEL BLOKE GÜNCELLE
    public async Task<bool> UpdateAsync(
        int id,
        EmlakBlokeTarihRequest request)
    {
        ValidateDates(
            request.BaslangicTarihi,
            request.BitisTarihi);

        var current = await _context.EmlakBlokeTarihler
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (current == null)
            return false;

        if (current.EmlakId != request.EmlakId)
        {
            throw new ArgumentException(
                "Blokenin emlak ID'si değiştirilemez.");
        }

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        await _takvimLock.LockAsync(current.EmlakId);

        // Kilitten sonra kaydı yeniden okuyoruz.
        var bloke = await _context.EmlakBlokeTarihler
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bloke == null)
            return false;

        if (bloke.EmlakId != current.EmlakId)
        {
            throw new InvalidOperationException(
                "Blokenin emlak bilgisi değişmiş.");
        }

        // Rezervasyondan oluşan bloke manuel değiştirilemez.
        if (bloke.RezervasyonId.HasValue)
        {
            throw new InvalidOperationException(
                "Rezervasyona bağlı bloke doğrudan güncellenemez. " +
                "Rezervasyon tarihlerini değiştirmelisiniz.");
        }

        bool conflict = await HasOverlapAsync(
            bloke.EmlakId,
            request.BaslangicTarihi,
            request.BitisTarihi,
            id);

        if (conflict)
        {
            throw new InvalidOperationException(
                "Yeni tarih aralığında başka bir bloke bulunuyor.");
        }

        bloke.BaslangicTarihi = request.BaslangicTarihi;
        bloke.BitisTarihi = request.BitisTarihi;
        bloke.Aciklama = request.Aciklama;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    // MANUEL BLOKE SİL
    public async Task<bool> DeleteAsync(int id)
    {
        // Önce emlak ID'sini öğreniyoruz.
        var current = await _context.EmlakBlokeTarihler
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (current == null)
            return false;

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        await _takvimLock.LockAsync(current.EmlakId);

        // Kilitten sonra tekrar okuyoruz.
        var bloke = await _context.EmlakBlokeTarihler
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bloke == null)
            return false;

        if (bloke.EmlakId != current.EmlakId)
        {
            throw new InvalidOperationException(
                "Blokenin emlak bilgisi değişmiş.");
        }

        if (bloke.RezervasyonId.HasValue)
        {
            throw new InvalidOperationException(
                "Rezervasyona bağlı bloke takvimden silinemez. " +
                "Önce rezervasyon durumunu değiştirin.");
        }

        _context.EmlakBlokeTarihler.Remove(bloke);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }
}
