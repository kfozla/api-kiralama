using System.Data;
using api_kiralama.Data;
using api_kiralama.Models;
using api_kiralama.Repositories;
using api_kiralama.Dtos;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class EmlakFiyatService
{
    private readonly EmlakFiyatRepository _repository;
    private readonly AppDbContext _context;
    private readonly EmlakTakvimLock _takvimLock;

    public EmlakFiyatService(
        EmlakFiyatRepository repository,
        AppDbContext context,
        EmlakTakvimLock takvimLock)
    {
        _repository = repository;
        _context = context;
        _takvimLock = takvimLock;
    }

    public Task<List<EmlakFiyat>> GetByEmlakIdAsync(int emlakId)
        => _repository.GetByEmlakIdAsync(emlakId);

    public Task<EmlakFiyat?> GetByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    private static void Validate(EmlakFiyatRequest request)
    {
        if (request.BaslangicTarihi >= request.BitisTarihi)
            throw new ArgumentException("Tarih aralığı geçersiz.");

        if (request.GecelikFiyat <= 0)
            throw new ArgumentException("Gecelik fiyat sıfırdan büyük olmalı.");
    }

    public async Task<EmlakFiyat> AddAsync(EmlakFiyatRequest request)
    {
        Validate(request);

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        await _takvimLock.LockAsync(request.EmlakId);

        if (await _repository.HasOverlapAsync(
                request.EmlakId,
                request.BaslangicTarihi,
                request.BitisTarihi))
        {
            throw new InvalidOperationException(
                "Bu tarihler için fiyat tanımlanmış.");
        }

        var fiyat = new EmlakFiyat
        {
            EmlakId = request.EmlakId,
            BaslangicTarihi = request.BaslangicTarihi,
            BitisTarihi = request.BitisTarihi,
            GecelikFiyat = request.GecelikFiyat
        };

        await _repository.AddAsync(fiyat);
        await transaction.CommitAsync();

        return fiyat;
    }

    public async Task<bool> UpdateAsync(
        int id, EmlakFiyatRequest request)
    {
        Validate(request);

        var current = await _context.EmlakFiyatlari
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (current == null)
            return false;

        if (current.EmlakId != request.EmlakId)
            throw new ArgumentException(
                "Fiyatın emlak ID'si değiştirilemez.");

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        await _takvimLock.LockAsync(current.EmlakId);

        var fiyat = await _repository.GetByIdAsync(id);

        if (fiyat == null)
            return false;

        // Kilit sonrası ilişkiyi tekrar kontrol et
        if (fiyat.EmlakId != current.EmlakId)
            throw new InvalidOperationException(
                "Fiyat kaydının bağlı olduğu emlak değişmiş.");

        if (await _repository.HasOverlapAsync(
                fiyat.EmlakId,
                request.BaslangicTarihi,
                request.BitisTarihi,
                id))
        {
            throw new InvalidOperationException(
                "Bu tarihler başka fiyat dönemiyle çakışıyor.");
        }

        fiyat.BaslangicTarihi = request.BaslangicTarihi;
        fiyat.BitisTarihi = request.BitisTarihi;
        fiyat.GecelikFiyat = request.GecelikFiyat;

        await _repository.UpdateAsync();

        await transaction.CommitAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var fiyat = await _repository.GetByIdAsync(id);
        if (fiyat == null)
            return false;

        await _takvimLock.LockAsync(fiyat.EmlakId);

        await _repository.DeleteAsync(fiyat);
        await transaction.CommitAsync();

        return true;
    }
}