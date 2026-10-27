using api_kiralama.Models;
using api_kiralama.Repositories;
using api_kiralama.Dtos;

namespace api_kiralama.Services;

public class EmlakService
{
    private readonly EmlakRepository _repository;
    private readonly HtmlContentService _htmlContentService;

    public EmlakService(EmlakRepository repository,HtmlContentService htmlContentService)
    {
        _repository = repository;
        _htmlContentService = htmlContentService;
    }

    private static EmlakResponse MapResponse(Emlak e)
    {
        return new EmlakResponse
        {
            Id = e.Id,
            EmlakSahibiId = e.EmlakSahibiId,
            BolgeId = e.BolgeId,
            EmlakTipi = e.EmlakTipi,
            EmlakAdı = e.EmlakAdı,
            Açıklama = e.Açıklama,
            Kapasite = e.Kapasite,
            YatakOdasıSayısı = e.YatakOdasıSayısı,
            BanyoSayısı = e.BanyoSayısı,
            GenelBilgiler = e.GenelBilgiler,
            OzellikIds = e.EmlakOzellikleri.Select(o => o.Id).ToList(),
            OneCikanOzellikIds =
                e.OneCıkanOzellikler.Select(o => o.Id).ToList(),
            HavuzBilgileri = e.HavuzBilgileri,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            GirisSaati = e.GirisSaati,
            CikisSaati = e.CikisSaati,
            HasarDepozitosu = e.HasarDepozitosu,
            TemizlikUcreti = e.TemizlikUcreti,
            MinimumKonaklamaSuresi = e.MinimumKonaklamaSuresi,
            MinimumKonaklamaTemizlik = e.MinimumKonaklamaTemizlik,
            
            Yayinda = e.Yayinda,
            OtoparkSayisi = e.OtoparkSayisi,
            BelgeNo = string.IsNullOrWhiteSpace(e.BelgeNo)
            ? null
            : e.BelgeNo.Trim(),
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
    public async Task<bool> UpdateYayinDurumuAsync(int id, bool yayinda)
    {
        return await _repository.UpdateYayinDurumuAsync(id, yayinda);
    }

    private async Task<List<EmlakOzellik>> ValidateAsync(
        EmlakRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmlakAdı))
            throw new ArgumentException("Emlak adı boş olamaz.");

        if (!await _repository.SahipExistsAsync(request.EmlakSahibiId) && request.EmlakSahibiId != null)
            throw new ArgumentException("Emlak sahibi bulunamadı.");

        if (!await _repository.BolgeExistsAsync(request.BolgeId))
            throw new ArgumentException("Bölge bulunamadı.");

        var ids = request.OzellikIds.Distinct().ToList();
        var oneCikanIds = request.OneCikanOzellikIds.Distinct().ToList();

        if (!oneCikanIds.All(ids.Contains))
            throw new ArgumentException(
                "Öne çıkan özellikler genel özellikler içinde bulunmalıdır.");

        var ozellikler = await _repository.GetOzelliklerAsync(ids);

        if (ozellikler.Count != ids.Count)
            throw new ArgumentException("Bazı özellikler bulunamadı.");

        return ozellikler;
    }

    public async Task<List<EmlakResponse>> GetAllAsync()
    {
        var emlaklar = await _repository.GetAllAsync();
        return emlaklar.Select(MapResponse).ToList();
    }

    public async Task<EmlakResponse?> GetByIdAsync(int id)
    {
        var emlak = await _repository.GetByIdAsync(id);

        return emlak == null ? null : MapResponse(emlak);
    }
    
    public async Task<List<EmlakResponse>> GetByEmlakSahibiIdAsync(int emlakSahibiId)
    {
        var emlaklar = await _repository.GetByEmlakSahibiIdAsync(emlakSahibiId);

        return emlaklar.Select(MapResponse).ToList();
    }

    public async Task<EmlakResponse> AddAsync(EmlakRequest request)
    {
        var ozellikler = await ValidateAsync(request);

        var featuredIds = request.OneCikanOzellikIds.ToHashSet();

        var emlak = new Emlak
        {
            EmlakSahibiId = request.EmlakSahibiId,
            BolgeId = request.BolgeId,
            EmlakTipi = request.EmlakTipi,
            EmlakAdı = request.EmlakAdı.Trim(),
            Açıklama = _htmlContentService.Sanitize(request.Açıklama),
            Kapasite = request.Kapasite,
            YatakOdasıSayısı = request.YatakOdasıSayısı,
            BanyoSayısı = request.BanyoSayısı,
            GenelBilgiler = _htmlContentService.Sanitize(request.GenelBilgiler),
            Latitude = request.Latitude,
            Longitude = request.Longitude,

            EmlakOzellikleri = ozellikler,
            OneCıkanOzellikler = ozellikler
                .Where(o => featuredIds.Contains(o.Id))
                .ToList(),
            
            HavuzBilgileri = request.HavuzBilgileri ?? new List<Havuz>(),
            
            GirisSaati = request.GirisSaati,
            CikisSaati = request.CikisSaati,
            HasarDepozitosu = request.HasarDepozitosu,
            TemizlikUcreti = request.TemizlikUcreti,
            MinimumKonaklamaSuresi = request.MinimumKonaklamaSuresi,
            MinimumKonaklamaTemizlik = request.MinimumKonaklamaTemizlik,
            
            Yayinda = request.Yayinda,
            OtoparkSayisi = request.OtoparkSayisi,
            BelgeNo = string.IsNullOrWhiteSpace(request.BelgeNo)
                ? null
                : request.BelgeNo.Trim(),
            
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _repository.AddAsync(emlak);

        return MapResponse(emlak);
    }

    public async Task<bool> UpdateAsync(
        int id, EmlakRequest request)
    {
        var emlak = await _repository.GetByIdAsync(id);

        if (emlak == null)
            return false;

        var ozellikler = await ValidateAsync(request);
        var featuredIds = request.OneCikanOzellikIds.ToHashSet();

        emlak.EmlakSahibiId = request.EmlakSahibiId;
        emlak.BolgeId = request.BolgeId;
        emlak.EmlakTipi = request.EmlakTipi;
        emlak.EmlakAdı = request.EmlakAdı.Trim();
        emlak.Açıklama = _htmlContentService.Sanitize(request.Açıklama);
        emlak.Kapasite = request.Kapasite;
        emlak.YatakOdasıSayısı = request.YatakOdasıSayısı;
        emlak.BanyoSayısı = request.BanyoSayısı;
        emlak.GenelBilgiler = _htmlContentService.Sanitize(request.GenelBilgiler);
        emlak.HavuzBilgileri = request.HavuzBilgileri;
        emlak.Latitude = request.Latitude;
        emlak.Longitude = request.Longitude;
        
        emlak.GirisSaati = request.GirisSaati;
        emlak.CikisSaati = request.CikisSaati;
        emlak.HasarDepozitosu = request.HasarDepozitosu;
        emlak.TemizlikUcreti = request.TemizlikUcreti;
        emlak.MinimumKonaklamaSuresi = request.MinimumKonaklamaSuresi;
        emlak.MinimumKonaklamaTemizlik = request.MinimumKonaklamaTemizlik;
        
        
        emlak.OtoparkSayisi = request.OtoparkSayisi;
        emlak.BelgeNo = string.IsNullOrWhiteSpace(request.BelgeNo)
            ? null
            : request.BelgeNo.Trim();
        emlak.UpdatedAt = DateTime.UtcNow;
        

        emlak.EmlakOzellikleri.Clear();

        foreach (var ozellik in ozellikler)
            emlak.EmlakOzellikleri.Add(ozellik);

        emlak.OneCıkanOzellikler.Clear();

        foreach (var ozellik in ozellikler)
        {
            if (featuredIds.Contains(ozellik.Id))
                emlak.OneCıkanOzellikler.Add(ozellik);
        }

        await _repository.UpdateAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var emlak = await _repository.GetByIdAsync(id);

        if (emlak == null)
            return false;

        await _repository.DeleteAsync(emlak);
        return true;
    }

    public async Task<List<EmlakResponse>> FilterAsync(
        int? bolgeId,
        EmlakTipi? emlakTipi,
        int? minKapasite,
        List<int>? ozellikIds)
    {
        var emlaklar = await _repository.FilterAsync(
            bolgeId, emlakTipi, minKapasite, ozellikIds);

        return emlaklar.Select(MapResponse).ToList();
    }
    
    public async Task<PagedResponse<EmlakListResponse>> GetPagedAsync(
        EmlakPagedRequest request)
    {
        return await _repository.GetPagedAsync(request);
    }
    
    
    public Task<List<EmlakSearchResponse>> SearchAsync(
        string query,
        int limit = 10)
    {
        return _repository.SearchAsync(query, limit);
    }

}