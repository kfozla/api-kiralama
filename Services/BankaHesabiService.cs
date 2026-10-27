
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class BankaHesabiService
{
    private const long MaxLogoSize = 2 * 1024 * 1024;

    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public BankaHesabiService(
        AppDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

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

    public async Task<BankaHesabi> CreateAsync(
        BankaHesabiRequest request)
    {
        var entity = new BankaHesabi();

        Apply(entity, request);

        _context.BankaHesaplari.Add(entity);

        await _context.SaveChangesAsync();

        return entity;
    }

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

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.BankaHesaplari
            .FindAsync(id);

        if (entity == null)
            return false;

        var oldLogo = entity.LogoUrl;

        _context.BankaHesaplari.Remove(entity);

        await _context.SaveChangesAsync();

        DeleteLocalLogo(oldLogo);

        return true;
    }

    public async Task<BankaHesabi?> UploadLogoAsync(
        int id,
        IFormFile file)
    {
        var entity = await _context.BankaHesaplari
            .FindAsync(id);

        if (entity == null)
            return null;

        if (file == null || file.Length == 0)
            throw new ArgumentException("Logo dosyası seçiniz.");

        if (file.Length > MaxLogoSize)
            throw new ArgumentException(
                "Logo dosyası en fazla 2 MB olabilir."
            );

        // Dosyanın gerçek formatını imzasından belirle.
        var extension = await DetectImageExtensionAsync(file);

        if (extension == null)
            throw new ArgumentException(
                "Yalnızca PNG, JPEG ve WebP kabul edilir."
            );

        var folder = GetLogoFolder();

        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(folder, fileName);

        try
        {
            await using (var stream = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            var oldLogo = entity.LogoUrl;

            entity.LogoUrl = $"/uploads/bankalar/{fileName}";
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            DeleteLocalLogo(oldLogo);

            return entity;
        }
        catch
        {
            if (File.Exists(path))
                File.Delete(path);

            throw;
        }
    }

    public async Task<BankaHesabi?> DeleteLogoAsync(int id)
    {
        var entity = await _context.BankaHesaplari
            .FindAsync(id);

        if (entity == null)
            return null;

        var oldLogo = entity.LogoUrl;

        entity.LogoUrl = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        DeleteLocalLogo(oldLogo);

        return entity;
    }

    private void Apply(
        BankaHesabi entity,
        BankaHesabiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BankaAdi) ||
            string.IsNullOrWhiteSpace(request.HesapSahibi))
        {
            throw new ArgumentException(
                "Banka adı ve hesap sahibi zorunludur."
            );
        }

        var iban = new string(
            request.Iban
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray()
        ).ToUpperInvariant();

        if (!IsValidIban(iban))
            throw new ArgumentException("Geçersiz IBAN.");

        var currency = request.ParaBirimi.Trim()
            .ToUpperInvariant();

        if (!new[] { "TRY", "EUR", "USD", "GBP" }
                .Contains(currency))
        {
            throw new ArgumentException(
                "Desteklenmeyen para birimi."
            );
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
            @"^[A-Z]{2}[0-9]{2}[A-Z0-9]+$"
        ))
            return false;

        // Ülkeye özgü temel uzunluk kontrolü.
        if (iban.StartsWith("TR") && iban.Length != 26)
            return false;

        // ISO 13616 MOD-97 kontrolü
        var rearranged = iban[4..] + iban[..4];

        int remainder = 0;

        foreach (char c in rearranged)
        {
            if (char.IsDigit(c))
            {
                remainder = (remainder * 10 + (c - '0')) % 97;
            }
            else
            {
                int value = c - 'A' + 10;

                remainder = (remainder * 100 + value) % 97;
            }
        }

        return remainder == 1;
    }

    private string GetLogoFolder()
    {
        var webRoot = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot"
            );
        }

        return Path.Combine(webRoot, "uploads", "bankalar");
    }

    private void DeleteLocalLogo(string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
            return;

        const string prefix = "/uploads/bankalar/";

        if (!logoUrl.StartsWith(prefix, StringComparison.Ordinal))
            return;

        var fileName = logoUrl[prefix.Length..];

        if (string.IsNullOrWhiteSpace(fileName) ||
            Path.GetFileName(fileName) != fileName)
            return;

        var path = Path.Combine(GetLogoFolder(), fileName);

        if (File.Exists(path))
            File.Delete(path);
    }

    private static async Task<string?> DetectImageExtensionAsync(
        IFormFile file)
    {
        byte[] header = new byte[12];

        await using var stream = file.OpenReadStream();

        int read = 0;

        while (read < header.Length)
        {
            int count = await stream.ReadAsync(
                header.AsMemory(read, header.Length - read)
            );

            if (count == 0)
                break;

            read += count;
        }

        // PNG
        byte[] png =
        {
            137, 80, 78, 71, 13, 10, 26, 10
        };

        if (read >= 8 && header[..8].SequenceEqual(png))
            return ".png";

        // JPEG
        if (read >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF)
            return ".jpg";

        // WebP: RIFF....WEBP
        if (read >= 12 &&
            System.Text.Encoding.ASCII.GetString(
                header, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(
                header, 8, 4) == "WEBP")
            return ".webp";

        return null;
    }
}
