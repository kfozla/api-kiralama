
using Amazon.S3;
using Amazon.S3.Model;
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class BankaHesabiS3Service
{
    private const long MaxLogoSize = 2 * 1024 * 1024;
    private const string LogoFolder = "bankalar";

    private readonly AppDbContext _context;
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly ILogger<BankaHesabiS3Service> _logger;

    public BankaHesabiS3Service(
        AppDbContext context,
        IAmazonS3 s3,
        IConfiguration configuration,
        ILogger<BankaHesabiS3Service> logger)
    {
        _context = context;
        _s3 = s3;
        _logger = logger;

        _bucketName = configuration["AWS:S3:BucketName"]
            ?? throw new InvalidOperationException(
                "AWS:S3:BucketName tanımlanmamış.");
    }

    // =========================
    // GET
    // =========================

    public async Task<List<BankaHesabi>> GetAllAsync(
        bool includeInactive = false)
    {
        var query = _context.BankaHesaplari
            .AsNoTracking()
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(x => x.AktifMi);

        return await query
            .OrderBy(x => x.Sira)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<BankaHesabi?> GetByIdAsync(int id)
    {
        return await _context.BankaHesaplari
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    // =========================
    // CREATE
    // =========================

    public async Task<BankaHesabi> CreateAsync(
        BankaHesabiRequest request)
    {
        var entity = new BankaHesabi();

        Apply(entity, request);

        _context.BankaHesaplari.Add(entity);

        await _context.SaveChangesAsync();

        return entity;
    }

    // =========================
    // UPDATE
    // =========================

    public async Task<BankaHesabi?> UpdateAsync(
        int id,
        BankaHesabiRequest request)
    {
        var entity = await _context.BankaHesaplari
            .FindAsync(id);

        if (entity == null)
            return null;

        Apply(entity, request);

        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return entity;
    }

    // =========================
    // DELETE BANK ACCOUNT
    // =========================

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.BankaHesaplari
            .FindAsync(id);

        if (entity == null)
            return false;

        string? oldLogoKey = entity.LogoUrl;

        _context.BankaHesaplari.Remove(entity);

        await _context.SaveChangesAsync();

        await DeleteS3LogoAsync(oldLogoKey);

        return true;
    }

    // =========================
    // UPLOAD LOGO
    // =========================

    public async Task<BankaHesabi?> UploadLogoAsync(
        int id,
        IFormFile file)
    {
        var entity = await _context.BankaHesaplari
            .FindAsync(id);

        if (entity == null)
            return null;

        if (file == null || file.Length == 0)
            throw new ArgumentException(
                "Logo dosyası seçiniz.");

        if (file.Length > MaxLogoSize)
            throw new ArgumentException(
                "Logo dosyası en fazla 2 MB olabilir.");

        string? extension =
            await DetectImageExtensionAsync(file);

        if (extension == null)
            throw new ArgumentException(
                "Yalnızca PNG, JPEG ve WebP kabul edilir.");

        string contentType = extension switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => throw new ArgumentException(
                "Desteklenmeyen dosya formatı.")
        };

        string objectKey =
            $"{LogoFolder}/{Guid.NewGuid():N}{extension}";

        string? oldLogoKey = entity.LogoUrl;

        // Önce dosyayı S3'e yükle.
        await using (var stream = file.OpenReadStream())
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = stream,
                ContentType = contentType,
                AutoCloseStream = false
            };

            await _s3.PutObjectAsync(request);
        }

        // SQL'e geçici URL değil S3 object key kaydet.
        entity.LogoUrl = objectKey;
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            // SQL başarısızsa yeni S3 logosunu temizle.
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
                    "Yeni banka logosu S3'ten temizlenemedi: {Key}",
                    objectKey);
            }

            throw;
        }

        // SQL başarılıysa eski logoyu sil.
        await DeleteS3LogoAsync(oldLogoKey);

        return entity;
    }

    // =========================
    // DELETE LOGO
    // =========================

    public async Task<BankaHesabi?> DeleteLogoAsync(int id)
    {
        var entity = await _context.BankaHesaplari
            .FindAsync(id);

        if (entity == null)
            return null;

        string? oldLogoKey = entity.LogoUrl;

        entity.LogoUrl = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await DeleteS3LogoAsync(oldLogoKey);

        return entity;
    }

    private async Task DeleteS3LogoAsync(
        string? objectKey)
    {
        if (!IsValidLogoKey(objectKey))
            return;

        try
        {
            await _s3.DeleteObjectAsync(
                _bucketName,
                objectKey!);
        }
        catch (Exception ex)
        {
            // SQL kaydı başarılı olabilir.
            // S3 temizleme hatasını logla.
            _logger.LogError(
                ex,
                "Banka logosu S3'ten silinemedi: {Key}",
                objectKey);
        }
    }

    // =========================
    // S3 URL
    // =========================

    private static bool IsValidLogoKey(string? objectKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
            return false;

        string prefix = $"{LogoFolder}/";

        if (!objectKey.StartsWith(
                prefix,
                StringComparison.Ordinal))
            return false;

        string fileName = objectKey[prefix.Length..];

        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName != Path.GetFileName(fileName) ||
            fileName.Contains('\\'))
            return false;

        return true;
    }

    public string? GetLogoUrl(string? objectKey)
    {
        if (!IsValidLogoKey(objectKey))
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

    // =========================
    // BANK ACCOUNT VALIDATION
    // =========================

    private static void Apply(
        BankaHesabi entity,
        BankaHesabiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BankaAdi) ||
            string.IsNullOrWhiteSpace(request.HesapSahibi))
        {
            throw new ArgumentException(
                "Banka adı ve hesap sahibi zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(request.Iban))
            throw new ArgumentException("IBAN zorunludur.");

        var iban = new string(
            request.Iban
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray()
        ).ToUpperInvariant();

        if (!IsValidIban(iban))
            throw new ArgumentException("Geçersiz IBAN.");

        if (string.IsNullOrWhiteSpace(request.ParaBirimi))
            throw new ArgumentException(
                "Para birimi zorunludur.");

        var currency = request.ParaBirimi
            .Trim()
            .ToUpperInvariant();

        if (!new[] { "TRY", "EUR", "USD", "GBP" }
                .Contains(currency))
        {
            throw new ArgumentException(
                "Desteklenmeyen para birimi.");
        }

        entity.BankaAdi = request.BankaAdi.Trim();
        entity.HesapSahibi = request.HesapSahibi.Trim();
        entity.Iban = iban;
        entity.SubeAdi = request.SubeAdi?.Trim();
        entity.ParaBirimi = currency;
        entity.Sira = request.Sira;
        entity.AktifMi = request.AktifMi;
    }

    private static bool IsValidIban(string iban)
    {
        if (iban.Length < 15 || iban.Length > 34)
            return false;

        if (!System.Text.RegularExpressions.Regex.IsMatch(
            iban,
            @"^[A-Z]{2}[0-9]{2}[A-Z0-9]+$"))
            return false;

        if (iban.StartsWith("TR") && iban.Length != 26)
            return false;

        var rearranged = iban[4..] + iban[..4];

        int remainder = 0;

        foreach (char c in rearranged)
        {
            if (c >= '0' && c <= '9')
            {
                remainder =
                    (remainder * 10 + (c - '0')) % 97;
            }
            else
            {
                int value = c - 'A' + 10;

                remainder =
                    (remainder * 100 + value) % 97;
            }
        }

        return remainder == 1;
    }

    // =========================
    // FILE SIGNATURE VALIDATION
    // =========================

    private static async Task<string?>
        DetectImageExtensionAsync(IFormFile file)
    {
        byte[] header = new byte[12];

        await using var stream = file.OpenReadStream();

        int read = 0;

        while (read < header.Length)
        {
            int count = await stream.ReadAsync(
                header.AsMemory(
                    read,
                    header.Length - read));

            if (count == 0)
                break;

            read += count;
        }

        byte[] png =
        {
            137, 80, 78, 71, 13, 10, 26, 10
        };

        if (read >= 8 &&
            header.AsSpan(0, 8).SequenceEqual(png))
            return ".png";

        if (read >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF)
            return ".jpg";

        if (read >= 12 &&
            System.Text.Encoding.ASCII.GetString(
                header, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(
                header, 8, 4) == "WEBP")
            return ".webp";

        return null;
    }
}
