
using Amazon.S3;
using Amazon.S3.Model;
using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Repositories;

namespace api_kiralama.Services;

public class EmlakOzellikS3Service
{
    private const long MaxLogoSize = 5 * 1024 * 1024;
    private const string LogoFolder = "emlak-ozellikler";

    private readonly EmlakOzellikRepository _repository;
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly ILogger<EmlakOzellikS3Service> _logger;

    public EmlakOzellikS3Service(
        EmlakOzellikRepository repository,
        IAmazonS3 s3,
        IConfiguration configuration,
        ILogger<EmlakOzellikS3Service> logger)
    {
        _repository = repository;
        _s3 = s3;
        _logger = logger;

        _bucketName = configuration["AWS:S3:BucketName"]
            ?? throw new InvalidOperationException(
                "AWS:S3:BucketName tanımlanmamış.");
    }

    // =========================
    // CRUD
    // =========================

    public async Task<List<EmlakOzellik>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<EmlakOzellik?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<EmlakOzellik>> GetByKategoriAsync(
        OzellikKategori kategori)
    {
        return await _repository.GetByKategoriAsync(kategori);
    }

    public async Task<EmlakOzellik> AddAsync(
        EmlakOzellikRequest request)
    {
        string ad = request.OzellikAdi?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(ad))
            throw new ArgumentException(
                "Özellik adı boş olamaz.");

        bool exists = await _repository.ExistsAsync(
            ad, request.Kategori);

        if (exists)
            throw new InvalidOperationException(
                "Bu kategoride aynı isimde bir özellik zaten mevcut.");

        var ozellik = new EmlakOzellik
        {
            OzellikAdi = ad,
            Kategori = request.Kategori
        };

        return await _repository.AddAsync(ozellik);
    }

    public async Task<bool> UpdateAsync(
        int id,
        EmlakOzellikRequest request)
    {
        var ozellik = await _repository.GetByIdAsync(id);

        if (ozellik == null)
            return false;

        string ad = request.OzellikAdi?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(ad))
            throw new ArgumentException(
                "Özellik adı boş olamaz.");

        bool exists = await _repository.ExistsAsync(
            ad, request.Kategori, id);

        if (exists)
            throw new InvalidOperationException(
                "Bu kategoride aynı isimde bir özellik zaten mevcut.");

        ozellik.OzellikAdi = ad;
        ozellik.Kategori = request.Kategori;

        await _repository.UpdateAsync(ozellik);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ozellik = await _repository.GetByIdAsync(id);

        if (ozellik == null)
            return false;

        bool isUsed = await _repository.IsUsedAsync(id);

        if (isUsed)
            throw new InvalidOperationException(
                "Bu özellik emlaklarda kullanıldığı için silinemez.");

        string? oldLogoKey = ozellik.LogoUrl;

        // Önce veritabanı kaydını kaldır.
        await _repository.DeleteAsync(ozellik);

        // Ardından S3 logosunu temizle.
        await DeleteS3LogoAsync(oldLogoKey);

        return true;
    }

    // =========================
    // LOGO VALIDATION
    // =========================

    private static async Task<(string Extension, string ContentType)>
        ValidateLogoAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException(
                "Logo dosyası seçilmelidir.");

        if (file.Length > MaxLogoSize)
            throw new ArgumentException(
                "Logo en fazla 5 MB olabilir.");

        byte[] header = new byte[12];

        await using (var stream = file.OpenReadStream())
        {
            int offset = 0;

            while (offset < header.Length)
            {
                int read = await stream.ReadAsync(
                    header.AsMemory(
                        offset,
                        header.Length - offset));

                if (read == 0)
                    break;

                offset += read;
            }

            if (offset < header.Length)
                throw new ArgumentException(
                    "Geçersiz görsel dosyası.");
        }

        bool isPng = header.AsSpan(0, 8).SequenceEqual(
            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

        bool isJpeg =
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF;

        bool isWebp =
            System.Text.Encoding.ASCII.GetString(
                header, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(
                header, 8, 4) == "WEBP";

        if (isPng)
            return (".png", "image/png");

        if (isJpeg)
            return (".jpg", "image/jpeg");

        if (isWebp)
            return (".webp", "image/webp");

        throw new ArgumentException(
            "Yalnızca PNG, JPG ve WebP dosyaları yüklenebilir.");
    }

    // =========================
    // UPLOAD LOGO
    // =========================

    public async Task<string> UploadLogoAsync(
        int ozellikId,
        IFormFile file)
    {
        var ozellik = await _repository
            .GetByIdForLogoAsync(ozellikId);

        if (ozellik == null)
            throw new KeyNotFoundException(
                "Emlak özelliği bulunamadı.");

        var (extension, contentType) =
            await ValidateLogoAsync(file);

        string objectKey =
            $"{LogoFolder}/{Guid.NewGuid():N}{extension}";

        string? oldLogoKey = ozellik.LogoUrl;

        // Önce S3'e yükle.
        await using (var stream = file.OpenReadStream())
        {
            await _s3.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey,
                    InputStream = stream,
                    ContentType = contentType,
                    AutoCloseStream = false
                });
        }

        // SQL'de geçici URL değil, S3 key saklanır.
        ozellik.LogoUrl = objectKey;

        try
        {
            await _repository.SaveChangesAsync();
        }
        catch
        {
            // SQL başarısız olursa yeni S3 nesnesini temizle.
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
                    "S3 upload rollback başarısız: {Key}",
                    objectKey);
            }

            throw;
        }

        // SQL başarılı olduktan sonra eski logoyu sil.
        await DeleteS3LogoAsync(oldLogoKey);

        return GetLogoUrl(objectKey)!;
    }

    // =========================
    // DELETE LOGO
    // =========================

    public async Task<bool> DeleteLogoAsync(int ozellikId)
    {
        var ozellik = await _repository
            .GetByIdForLogoAsync(ozellikId);

        if (ozellik == null)
            return false;

        string? oldLogoKey = ozellik.LogoUrl;

        ozellik.LogoUrl = null;

        await _repository.SaveChangesAsync();

        await DeleteS3LogoAsync(oldLogoKey);

        return true;
    }

    private async Task DeleteS3LogoAsync(string? objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            return;

        string prefix = $"{LogoFolder}/";

        if (!objectKey.StartsWith(
                prefix,
                StringComparison.Ordinal))
            return;

        string fileName = objectKey[prefix.Length..];

        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName != Path.GetFileName(fileName) ||
            fileName.Contains('\\'))
            return;

        try
        {
            await _s3.DeleteObjectAsync(
                _bucketName,
                objectKey);
        }
        catch (Exception ex)
        {
            // SQL kaydını geri alamayacağımız için logla.
            _logger.LogError(
                ex,
                "S3 logo silinemedi: {Key}",
                objectKey);
        }
    }

    // =========================
    // PRESIGNED URL
    // =========================

    public string? GetLogoUrl(string? objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            return null;

        if (!objectKey.StartsWith(
                $"{LogoFolder}/",
                StringComparison.Ordinal))
            return null;

        return _s3.GetPreSignedURL(
            new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });
    }
}
