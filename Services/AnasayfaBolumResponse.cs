
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Repositories;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class AnasayfaBolumService
{
    private readonly AnasayfaBolumRepository _repository;
    private readonly AppDbContext _context;

    public AnasayfaBolumService(
        AnasayfaBolumRepository repository,
        AppDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    // ADMIN - TÜM BÖLÜMLER
    public async Task<List<AnasayfaBolumResponse>> GetAllAsync()
    {
        var bolumler = await _repository.GetAllAsync();

        return bolumler
            .Select(MapResponse)
            .ToList();
    }

    // ADMIN - TEK BÖLÜM
    public async Task<AnasayfaBolumResponse?> GetByIdAsync(int id)
    {
        var bolum = await _repository.GetByIdAsync(id);

        return bolum == null ? null : MapResponse(bolum);
    }

    // ADMIN - YENİ BÖLÜM
    public async Task<AnasayfaBolumResponse> AddAsync(
        AnasayfaBolumRequest request)
    {
        ValidateRequest(request);

        string slug = NormalizeSlug(request.Slug);

        if (await _repository.SlugExistsAsync(slug))
        {
            throw new ArgumentException(
                "Bu slug ile başka bir anasayfa bölümü bulunuyor.");
        }

        var bolum = new AnasayfaBolum
        {
            Baslik = request.Baslik.Trim(),
            Aciklama = NormalizeDescription(request.Aciklama),
            Slug = slug,
            GosterimTipi = request.GosterimTipi,
            SecimTipi = request.SecimTipi,
            MaksimumVillaSayisi = request.MaksimumVillaSayisi,
            Sira = request.Sira,
            Aktif = request.Aktif
        };

        await _repository.AddAsync(bolum);

        return MapResponse(bolum);
    }

    // ADMIN - BÖLÜM GÜNCELLE
    public async Task<bool> UpdateAsync(
        int id,
        AnasayfaBolumRequest request)
    {
        ValidateRequest(request);

        var bolum = await _repository.GetByIdAsync(id);

        if (bolum == null)
            return false;

        string slug = NormalizeSlug(request.Slug);

        if (await _repository.SlugExistsAsync(slug, id))
        {
            throw new ArgumentException(
                "Bu slug başka bir anasayfa bölümünde kullanılıyor.");
        }

        bolum.Baslik = request.Baslik.Trim();
        bolum.Aciklama = NormalizeDescription(request.Aciklama);
        bolum.Slug = slug;
        bolum.GosterimTipi = request.GosterimTipi;
        bolum.SecimTipi = request.SecimTipi;
        bolum.MaksimumVillaSayisi = request.MaksimumVillaSayisi;
        bolum.Sira = request.Sira;
        bolum.Aktif = request.Aktif;

        // Otomatik seçimde eski manuel kayıtları temizle.
        if (bolum.SecimTipi == AnasayfaSecimTipi.SonEklenenler)
        {
            _repository.RemoveBolumEmlaklar(bolum.Emlaklar);
        }
        else if (bolum.Emlaklar.Count >
                 bolum.MaksimumVillaSayisi)
        {
            throw new ArgumentException(
                "Seçili villa sayısı yeni maksimum sınırı aşıyor.");
        }

        await _repository.SaveChangesAsync();

        return true;
    }

    // ADMIN - BÖLÜM SİL
    public async Task<bool> DeleteAsync(int id)
    {
        var bolum = await _repository.GetByIdAsync(id);

        if (bolum == null)
            return false;

        await _repository.DeleteAsync(bolum);

        return true;
    }

    // ADMIN - MANUEL VİLLALARI TOPLU KAYDET
    public async Task<bool> SaveEmlaklarAsync(
        int bolumId,
        AnasayfaBolumEmlaklarRequest request)
    {
        if (request == null || request.Emlaklar == null)
        {
            throw new ArgumentException(
                "Villa listesi gönderilmelidir.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var bolum = await _repository.GetByIdAsync(bolumId);

        if (bolum == null)
            return false;

        if (bolum.SecimTipi != AnasayfaSecimTipi.Manuel)
        {
            throw new InvalidOperationException(
                "Otomatik seçim yapılan bölüme manuel villa eklenemez.");
        }

        var items = request.Emlaklar;

        if (items.Count > bolum.MaksimumVillaSayisi)
        {
            throw new ArgumentException(
                $"Bu bölümde en fazla " +
                $"{bolum.MaksimumVillaSayisi} villa seçilebilir.");
        }

        if (items.Any(x => x.EmlakId <= 0 || x.Sira < 0))
        {
            throw new ArgumentException(
                "Villa ID veya sıralama bilgisi geçersiz.");
        }

        var ids = items.Select(x => x.EmlakId).ToList();

        if (ids.Distinct().Count() != ids.Count)
        {
            throw new ArgumentException(
                "Aynı villa bir bölüme birden fazla eklenemez.");
        }

        if (!await _repository.EmlaklarExistAsync(ids))
        {
            throw new ArgumentException(
                "Seçilen villalardan bazıları bulunamadı veya silinmiş.");
        }

        // Gönderilen sıraya göre düzenle.
        var siraliEmlaklar = items
            .Select((item, index) => new
            {
                item.EmlakId,
                item.Sira,
                Index = index
            })
            .OrderBy(x => x.Sira)
            .ThenBy(x => x.Index)
            .ToList();

        // Eski ilişkileri sil.
        _repository.RemoveBolumEmlaklar(
            bolum.Emlaklar.ToList());

        // Silme işlemini aynı transaction içinde uygula.
        // Unique index çakışmasını önler.
        await _repository.SaveChangesAsync();

        // Yeni ilişkileri oluştur.
        var yeniEmlaklar = siraliEmlaklar
            .Select((x, index) => new AnasayfaBolumEmlak
            {
                AnasayfaBolumId = bolumId,
                EmlakId = x.EmlakId,
                Sira = index + 1
            })
            .ToList();

        await _repository.AddBolumEmlaklarAsync(yeniEmlaklar);

        await _repository.SaveChangesAsync();

        await transaction.CommitAsync();

        return true;
    }

    // PUBLIC - AKTİF ANASAYFA BÖLÜMLERİ
    public async Task<List<AnasayfaPublicBolumResponse>>
        GetPublicAsync()
    {
        var bolumler = await _repository.GetActiveAsync();

        var result = new List<AnasayfaPublicBolumResponse>();

        foreach (var bolum in bolumler)
        {
            List<Emlak> emlaklar;

            if (bolum.SecimTipi ==
                AnasayfaSecimTipi.SonEklenenler)
            {
                // Otomatik en yeni villalar
                emlaklar = await _repository
                    .GetLatestEmlaklarAsync(
                        bolum.MaksimumVillaSayisi);
            }
            else
            {
                // Admin tarafından seçilen villalar
                var siraliIds = bolum.Emlaklar
                    .OrderBy(x => x.Sira)
                    .ThenBy(x => x.Id)
                    .Take(bolum.MaksimumVillaSayisi)
                    .Select(x => x.EmlakId)
                    .ToList();

                var bulunanEmlaklar =
                    await _repository.GetEmlaklarByIdsAsync(
                        siraliIds);

                var emlakMap = bulunanEmlaklar
                    .ToDictionary(x => x.Id);

                // SQL IN sonucu sıralı gelmez.
                // Adminin belirlediği sıralamayı koruyoruz.
                emlaklar = siraliIds
                    .Where(id => emlakMap.ContainsKey(id))
                    .Select(id => emlakMap[id])
                    .ToList();
            }

            result.Add(new AnasayfaPublicBolumResponse
            {
                Id = bolum.Id,
                Baslik = bolum.Baslik,
                Slug = bolum.Slug,
                Aciklama = bolum.Aciklama,
                GosterimTipi = bolum.GosterimTipi,
                Sira = bolum.Sira,
                Emlaklar = emlaklar
                    .Select(MapPublicEmlak)
                    .ToList()
            });
        }

        return result;
    }

    // VALIDATION
    private static void ValidateRequest(
        AnasayfaBolumRequest request)
    {
        if (request == null)
            throw new ArgumentException("Bölüm bilgileri gereklidir.");

        if (string.IsNullOrWhiteSpace(request.Baslik))
            throw new ArgumentException("Bölüm başlığı zorunludur.");

        if (request.Baslik.Trim().Length > 150)
            throw new ArgumentException(
                "Bölüm başlığı en fazla 150 karakter olabilir.");

        if (string.IsNullOrWhiteSpace(request.Slug))
            throw new ArgumentException("Slug zorunludur.");

        if (request.Slug.Trim().Length > 150)
            throw new ArgumentException(
                "Slug en fazla 150 karakter olabilir.");

        if (request.Aciklama?.Trim().Length > 500)
            throw new ArgumentException(
                "Açıklama en fazla 500 karakter olabilir.");

        if (!Enum.IsDefined(
                typeof(AnasayfaGosterimTipi),
                request.GosterimTipi))
        {
            throw new ArgumentException(
                "Geçersiz gösterim tipi.");
        }

        if (!Enum.IsDefined(
                typeof(AnasayfaSecimTipi),
                request.SecimTipi))
        {
            throw new ArgumentException(
                "Geçersiz seçim tipi.");
        }

        if (request.MaksimumVillaSayisi < 1 ||
            request.MaksimumVillaSayisi > 100)
        {
            throw new ArgumentException(
                "Maksimum villa sayısı 1 ile 100 arasında olmalıdır.");
        }

        if (request.Sira < 0)
            throw new ArgumentException(
                "Bölüm sırası negatif olamaz.");
    }

    private static string NormalizeSlug(string slug)
    {
        return slug.Trim().ToLowerInvariant();
    }

    private static string? NormalizeDescription(
        string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    // ADMIN RESPONSE MAPPING
    private static AnasayfaBolumResponse MapResponse(
        AnasayfaBolum bolum)
    {
        return new AnasayfaBolumResponse
        {
            Id = bolum.Id,
            Baslik = bolum.Baslik,
            Aciklama = bolum.Aciklama,
            Slug = bolum.Slug,
            GosterimTipi = bolum.GosterimTipi,
            SecimTipi = bolum.SecimTipi,
            Sira = bolum.Sira,
            Aktif = bolum.Aktif,
            MaksimumVillaSayisi = bolum.MaksimumVillaSayisi,

            EmlakIds = bolum.Emlaklar
                .OrderBy(x => x.Sira)
                .ThenBy(x => x.Id)
                .Select(x => x.EmlakId)
                .ToList()
        };
    }

    // PUBLIC VILLA MAPPING
    private static AnasayfaPublicEmlakResponse MapPublicEmlak(
        Emlak emlak)
    {
        return new AnasayfaPublicEmlakResponse
        {
            Id = emlak.Id,
            EmlakAdi = emlak.EmlakAdı,
            BolgeId = emlak.BolgeId,
            Kapasite = emlak.Kapasite,
            YatakOdasiSayisi = emlak.YatakOdasıSayısı,
            BanyoSayisi = emlak.BanyoSayısı
        };
    }
}
