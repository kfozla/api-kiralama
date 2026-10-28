
using Amazon.S3;
using Amazon.S3.Model;
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class EmlakFotografS3Service
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private const int MaxFileCount = 99;

    private readonly EmlakFotografRepository _repository;
    private readonly AppDbContext _context;
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly ILogger<EmlakFotografS3Service> _logger;

    public EmlakFotografS3Service(
        EmlakFotografRepository repository,
        AppDbContext context,
        IAmazonS3 s3,
        IConfiguration configuration,
        ILogger<EmlakFotografS3Service> logger)
    {
        _repository = repository;
        _context = context;
        _s3 = s3;
        _logger = logger;

        _bucketName = configuration["AWS:S3:BucketName"]
            ?? throw new InvalidOperationException(
                "AWS:S3:BucketName tanımlanmamış.");
    }

    private static string GetObjectKey(int emlakId, string extension)
    {
        return $"emlaklar/{emlakId}/{Guid.NewGuid():N}{extension}";
    }

    public string? GetPhotoUrl(string? objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            return null;

        if (!objectKey.StartsWith(
                "emlaklar/",
                StringComparison.Ordinal))
            return null;

        return _s3.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });
    }

    private EmlakFotografResponse ToResponse(EmlakFotograf fotograf)
    {
        return new EmlakFotografResponse
        {
            Id = fotograf.Id,
            EmlakId = fotograf.EmlakId,
            DosyaYolu = GetPhotoUrl(fotograf.DosyaYolu),
            Sira = fotograf.Sira,
            KapakMi = fotograf.KapakMi,
            CreatedAt = fotograf.CreatedAt
        };
    }

    private async Task EnsureEmlakExistsAsync(int emlakId)
    {
        if (!await _repository.EmlakExistsAsync(emlakId))
        {
            throw new KeyNotFoundException("Emlak bulunamadı.");
        }
    }

    private static async Task<string> DetectExtensionAsync(IFormFile file)
    {
        if (file.Length <= 0 || file.Length > MaxFileSize)
        {
            throw new ArgumentException(
                "Fotoğraf boyutu 0 ile 10 MB arasında olmalıdır.");
        }

        byte[] header = new byte[12];

        await using var stream = file.OpenReadStream();

        var count = await stream.ReadAsync(
            header.AsMemory(0, header.Length));

        if (count >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF)
        {
            return ".jpg";
        }

        if (count >= 8 &&
            header.AsSpan(0, 8).SequenceEqual(
                new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
        {
            return ".png";
        }

        if (count >= 12 &&
            System.Text.Encoding.ASCII.GetString(
                header, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(
                header, 8, 4) == "WEBP")
        {
            return ".webp";
        }

        throw new ArgumentException(
            "Yalnızca PNG, JPEG ve WebP fotoğrafları yüklenebilir.");
    }

    private static string GetContentType(string extension)
    {
        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => throw new ArgumentException(
                "Desteklenmeyen fotoğraf formatı.")
        };
    }

    public async Task<List<EmlakFotografResponse>> GetAsync(int emlakId)
    {
        await EnsureEmlakExistsAsync(emlakId);

        var fotograflar = await _repository.GetByEmlakIdAsync(emlakId);

        return fotograflar.Select(ToResponse).ToList();
    }

    public async Task<List<EmlakFotografResponse>> UploadAsync(
        int emlakId,
        List<IFormFile> files)
    {
        await EnsureEmlakExistsAsync(emlakId);

        if (files == null ||
            files.Count == 0 ||
            files.Count > MaxFileCount)
        {
            throw new ArgumentException(
                "Tek seferde 1-99 fotoğraf yükleyebilirsiniz.");
        }

        var validated = new List<(IFormFile File, string Extension)>();

        foreach (var file in files)
        {
            var extension = await DetectExtensionAsync(file);
            validated.Add((file, extension));
        }

        var existing = await _repository.GetTrackedAsync(emlakId);

        var nextOrder = existing.Count == 0
            ? 0
            : existing.Max(f => f.Sira) + 1;

        var hasCover = existing.Any(f => f.KapakMi);

        var uploadedKeys = new List<string>();
        var newPhotos = new List<EmlakFotograf>();

        try
        {
            foreach (var (file, extension) in validated)
            {
                var objectKey = GetObjectKey(emlakId, extension);

                await using var stream = file.OpenReadStream();

                var uploadRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey,
                    InputStream = stream,
                    ContentType = GetContentType(extension),
                    AutoCloseStream = false
                };

                await _s3.PutObjectAsync(uploadRequest);

                uploadedKeys.Add(objectKey);

                newPhotos.Add(new EmlakFotograf
                {
                    EmlakId = emlakId,
                    DosyaYolu = objectKey,
                    Sira = nextOrder++,
                    KapakMi = !hasCover && newPhotos.Count == 0,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _repository.AddRange(newPhotos);
            await _repository.SaveChangesAsync();
        }
        catch
        {
            // S3 upload veya SQL kaydı başarısızsa
            // bu istekte yüklenen yeni dosyaları temizle.
            foreach (var key in uploadedKeys)
            {
                try
                {
                    await _s3.DeleteObjectAsync(
                        _bucketName,
                        key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "S3 cleanup başarısız. ObjectKey: {ObjectKey}",
                        key);
                }
            }

            throw;
        }

        // SQL kaydından sonra ID değerleri oluşmuş olur.
        // URL üretimini cleanup bloğu dışında tutuyoruz.
        return newPhotos.Select(ToResponse).ToList();
    }

    public async Task DeleteAsync(int emlakId, int fotografId)
    {
        await EnsureEmlakExistsAsync(emlakId);

        string objectKey;

        await using (var transaction =
            await _context.Database.BeginTransactionAsync())
        {
            var allPhotos =
                await _repository.GetTrackedAsync(emlakId);

            var photo = allPhotos.FirstOrDefault(
                f => f.Id == fotografId);

            if (photo == null)
            {
                throw new KeyNotFoundException(
                    "Fotoğraf bulunamadı.");
            }

            objectKey = photo.DosyaYolu;

            var wasCover = photo.KapakMi;

            _repository.Delete(photo);

            await _repository.SaveChangesAsync();

            if (wasCover)
            {
                var replacement = allPhotos
                    .Where(f => f.Id != fotografId)
                    .OrderBy(f => f.Sira)
                    .ThenBy(f => f.Id)
                    .FirstOrDefault();

                if (replacement != null)
                {
                    replacement.KapakMi = true;
                }
            }

            var order = 0;

            foreach (var item in allPhotos
                .Where(f => f.Id != fotografId)
                .OrderBy(f => f.Sira)
                .ThenBy(f => f.Id))
            {
                item.Sira = order++;
            }

            await _repository.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        // SQL transaction S3 işlemlerini kapsamaz.
        // Bu nedenle SQL commit sonrasında S3'ten silinir.
        try
        {
            await _s3.DeleteObjectAsync(
                _bucketName,
                objectKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "SQL kaydı silindi ancak S3 dosyası silinemedi: {ObjectKey}",
                objectKey);
        }
    }

    public async Task SetCoverAsync(int emlakId, int fotografId)
    {
        await EnsureEmlakExistsAsync(emlakId);

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var photos = await _repository.GetTrackedAsync(emlakId);

        var selected = photos.FirstOrDefault(
            f => f.Id == fotografId);

        if (selected == null)
        {
            throw new KeyNotFoundException(
                "Fotoğraf bulunamadı.");
        }

        if (selected.KapakMi)
            return;

        // Tek kapak fotoğrafı index'i için önce
        // eski kapak kaldırılır ve kaydedilir.
        foreach (var photo in photos.Where(f => f.KapakMi))
        {
            photo.KapakMi = false;
        }

        await _repository.SaveChangesAsync();

        selected.KapakMi = true;

        await _repository.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task ReorderAsync(
        int emlakId,
        List<int> fotografIds)
    {
        await EnsureEmlakExistsAsync(emlakId);

        var photos = await _repository.GetTrackedAsync(emlakId);

        if (fotografIds == null ||
            fotografIds.Count != photos.Count ||
            fotografIds.Distinct().Count() != photos.Count ||
            !fotografIds.OrderBy(x => x)
                .SequenceEqual(
                    photos.Select(p => p.Id).OrderBy(x => x)))
        {
            throw new ArgumentException(
                "Fotoğraf listesi eksik veya geçersiz.");
        }

        var positions = fotografIds
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);

        foreach (var photo in photos)
        {
            photo.Sira = positions[photo.Id];
        }

        await _repository.SaveChangesAsync();
    }
}
