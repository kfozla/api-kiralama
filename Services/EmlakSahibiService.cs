using api_kiralama.Models;
using api_kiralama.Repositories;
using api_kiralama.Dtos;

namespace api_kiralama.Services;

public class EmlakSahibiService
{
    private readonly EmlakSahibiRepository _repository;

    public EmlakSahibiService(EmlakSahibiRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EmlakSahibi>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }
    
    
    public async Task<PagedResponse<EmlakSahibiListResponse>> GetPagedAsync(
        EmlakSahibiPagedRequest request)
    {
        return await _repository.GetPagedAsync(request);
    }

    
    
    public async Task<List<EmlakSahibiSearchResponse>> SearchAsync(
        string query,
        int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) ||
            query.Trim().Length < 2)
        {
            return new List<EmlakSahibiSearchResponse>();
        }

        return await _repository.SearchAsync(
            query,
            limit
        );
    }


    public async Task<EmlakSahibi?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<EmlakSahibi> AddAsync(EmlakSahibiRequest request)
    {
        var emlakSahibi = new EmlakSahibi
        {
            Ad = request.Ad,
            Soyad = request.Soyad,
            Telefon = request.Telefon,
            Email = request.Email,
            Aciklama = request.Aciklama,
            KayitTarihi = DateTime.UtcNow
        };

        return await _repository.AddAsync(emlakSahibi);
    }

    public async Task<bool> UpdateAsync(
        int id, EmlakSahibiRequest request)
    {
        var emlakSahibi = await _repository.GetByIdAsync(id);

        if (emlakSahibi == null)
            return false;

        emlakSahibi.Ad = request.Ad;
        emlakSahibi.Soyad = request.Soyad;
        emlakSahibi.Telefon = request.Telefon;
        emlakSahibi.Email = request.Email;
        emlakSahibi.Aciklama = request.Aciklama;

        await _repository.UpdateAsync(emlakSahibi);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var emlakSahibi = await _repository.GetByIdAsync(id);

        if (emlakSahibi == null)
            return false;

        bool hasEmlak = await _repository.HasEmlakAsync(id);

        if (hasEmlak)
            throw new InvalidOperationException(
                "Bu emlak sahibine bağlı emlaklar bulunduğu için silinemez."
            );

        await _repository.DeleteAsync(emlakSahibi);

        return true;
    }
}