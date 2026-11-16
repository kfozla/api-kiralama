
using System.Security.Cryptography;
using System.Text;
using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Repositories;

namespace api_kiralama.Services;

public class EmlakPaylasimService
{
    
    private readonly EmlakPaylasimRepository _repository;
    private readonly EmlakFotografS3Service _fotografService;

    public EmlakPaylasimService(
        EmlakPaylasimRepository repository,
        EmlakFotografS3Service fotografService)
    {
        _repository = repository;
        _fotografService = fotografService;
    }


    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);

        return Convert.ToHexString(
            SHA256.HashData(bytes)
        );
    }

    public async Task<EmlakPaylasimOlusturResponse> OlusturAsync(
        EmlakPaylasimOlusturRequest request)
    {
        var ids = (request.EmlakIds ?? new List<int>())
            .Distinct()
            .ToList();

        if (ids.Count == 0 || ids.Count > 50 ||
            ids.Any(id => id <= 0))
        {
            throw new ArgumentException(
                "1 ile 50 arasında geçerli emlak seçmelisiniz.");
        }

        if (request.Baslik?.Length > 150)
        {
            throw new ArgumentException(
                "Başlık en fazla 150 karakter olabilir.");
        }

        if (request.GirisTarihi.HasValue !=
            request.CikisTarihi.HasValue)
        {
            throw new ArgumentException(
                "Giriş ve çıkış tarihleri birlikte girilmelidir.");
        }

        if (request.GirisTarihi.HasValue &&
            request.CikisTarihi!.Value <=
            request.GirisTarihi.Value)
        {
            throw new ArgumentException(
                "Çıkış tarihi giriş tarihinden sonra olmalıdır.");
        }

        var yayindakiIds =
            await _repository.GetYayindakiEmlakIdsAsync(ids);

        if (yayindakiIds.Count != ids.Count)
        {
            throw new ArgumentException(
                "Seçilen emlaklardan bazıları yayında değil veya bulunamadı.");
        }

        // 32 byte kriptografik rastgele token
        var token = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32)
        );

        var now = DateTime.UtcNow;

        var paylasim = new EmlakPaylasim
        {
            TokenHash = HashToken(token),

            Baslik = string.IsNullOrWhiteSpace(request.Baslik)
                ? "Size Özel Villa Önerileri"
                : request.Baslik.Trim(),

            GirisTarihi = request.GirisTarihi,
            CikisTarihi = request.CikisTarihi,

            CreatedAt = now,
            ExpiresAt = now.AddDays(30),

            IsRevoked = false,

            Items = ids.Select((id, index) =>
                new EmlakPaylasimItem
                {
                    EmlakId = id,
                    Sira = index
                }
            ).ToList()
        };

        await _repository.AddAsync(paylasim);

        return new EmlakPaylasimOlusturResponse
        {
            Id = paylasim.Id,
            Token = token,
            ExpiresAt = paylasim.ExpiresAt
        };
    }

    public async Task<EmlakPaylasimDetayResponse?> GetPublicAsync(
        string token)
    {
        if (string.IsNullOrWhiteSpace(token) ||
            token.Length != 64 ||
            !token.All(Uri.IsHexDigit))
        {
            return null;
        }

        var tokenHash = HashToken(
            token.ToUpperInvariant()
        );

        var paylasim =
            await _repository.GetByTokenHashAsync(tokenHash);

        if (paylasim == null)
        {
            return null;
        }

        var emlaklar =
            await _repository.GetPaylasilanEmlaklarAsync(paylasim.Id);

        foreach (var emlak in emlaklar)
        {
            emlak.KapakFotografUrl =
                _fotografService.GetPhotoUrl(
                    emlak.KapakFotografUrl
                );
        }

        return new EmlakPaylasimDetayResponse
        {
            Baslik = paylasim.Baslik,
            GirisTarihi = paylasim.GirisTarihi,
            CikisTarihi = paylasim.CikisTarihi,
            ExpiresAt = paylasim.ExpiresAt,
            Emlaklar = emlaklar
        };
    }
}
