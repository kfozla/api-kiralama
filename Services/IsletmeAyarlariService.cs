
using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Repositories;

namespace api_kiralama.Services;

public class IsletmeAyarlariService
{
    private readonly IsletmeAyarlariRepository _repository;
    private readonly string _storagePath;

    private const long MaxLogoSize = 5 * 1024 * 1024;
    private const long MaxBelgeSize = 10 * 1024 * 1024;

    public IsletmeAyarlariService(
        IsletmeAyarlariRepository repository,
        IHostEnvironment environment)
    {
        _repository = repository;

        _storagePath = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "isletme"
        );
    }

    private static IsletmeAyarlariResponse ToResponse(
        IsletmeAyarlari item)
    {
        return new IsletmeAyarlariResponse
        {
            Id = item.Id,
            IsletmeAdi = item.IsletmeAdi,
            YetkiliKisi = item.YetkiliKisi,
            Telefon = item.Telefon,
            Email = item.Email,
            Adres = item.Adres,
            WebSitesi = item.WebSitesi,
            LogoVar = !string.IsNullOrWhiteSpace(item.LogoUrl),
            TursabUyeligiVar = item.TursabUyeligiVar,
            TursabBelgeNo = item.TursabBelgeNo,
            TursabBelgesiVar =
                !string.IsNullOrWhiteSpace(item.TursabBelgeUrl),
            UpdatedAt = item.UpdatedAt
        };
    }

    public async Task<IsletmeAyarlariResponse> GetAsync()
    {
        var item = await _repository.GetOrCreateAsync();

        return ToResponse(item);
    }

    public async Task<IsletmeAyarlariResponse> UpdateAsync(
        IsletmeAyarlariRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IsletmeAdi))
        {
            throw new ArgumentException(
                "İşletme adı boş bırakılamaz.");
        }

        var item = await _repository.GetOrCreateAsync();

        item.IsletmeAdi = request.IsletmeAdi.Trim();
        item.YetkiliKisi = request.YetkiliKisi?.Trim();
        item.Telefon = request.Telefon?.Trim();
        item.Email = request.Email?.Trim();
        item.Adres = request.Adres?.Trim();
        item.WebSitesi = request.WebSitesi?.Trim();

        item.TursabUyeligiVar = request.TursabUyeligiVar;
        item.TursabBelgeNo = request.TursabUyeligiVar
            ? request.TursabBelgeNo?.Trim()
            : null;

        // Üyelik kapatıldığında belgeyi kendiliğinden silmiyoruz.
        // Silme işlemi ayrı DELETE endpointiyle yapılır.
        item.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync();

        return ToResponse(item);
    }

    private static string DetectFileType(
        byte[] header,
        bool isLogo)
    {
        bool png =
            header.Length >= 8 &&
            header[0] == 0x89 &&
            header[1] == 0x50 &&
            header[2] == 0x4E &&
            header[3] == 0x47 &&
            header[4] == 0x0D &&
            header[5] == 0x0A &&
            header[6] == 0x1A &&
            header[7] == 0x0A;

        bool jpg =
            header.Length >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF;

        bool webp =
            header.Length >= 12 &&
            System.Text.Encoding.ASCII.GetString(
                header, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(
                header, 8, 4) == "WEBP";

        bool pdf =
            header.Length >= 5 &&
            System.Text.Encoding.ASCII.GetString(
                header, 0, 5) == "%PDF-";

        if (png) return ".png";
        if (jpg) return ".jpg";
        if (isLogo && webp) return ".webp";
        if (!isLogo && pdf) return ".pdf";

        throw new ArgumentException(
            isLogo
                ? "Logo PNG, JPG veya WebP olmalıdır."
                : "TÜRSAB belgesi PDF, PNG veya JPG olmalıdır.");
    }

    private static string ContentTypeFor(string extension)
    {
        return extension switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }

    private async Task<string> StoreFileAsync(
        IFormFile file,
        bool isLogo)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Dosya seçilmedi.");

        long maxSize = isLogo ? MaxLogoSize : MaxBelgeSize;

        if (file.Length > maxSize)
        {
            throw new ArgumentException(
                isLogo
                    ? "Logo en fazla 5 MB olabilir."
                    : "TÜRSAB belgesi en fazla 10 MB olabilir.");
        }

        await using var source = file.OpenReadStream();

        byte[] header = new byte[12];

        int read = 0;
        while (read < header.Length)
        {
            int n = await source.ReadAsync(
                header.AsMemory(read, header.Length - read));

            if (n == 0) break;
            read += n;
        }

        string extension = DetectFileType(
            header[..read], isLogo);

        Directory.CreateDirectory(_storagePath);

        string name = $"{Guid.NewGuid():N}{extension}";
        string path = Path.Combine(_storagePath, name);

        try
        {
            await using var destination = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

            await destination.WriteAsync(header.AsMemory(0, read));
            await source.CopyToAsync(destination);

            return name;
        }
        catch
        {
            DeleteStoredFile(name);
            throw;
        }
    }

    private void DeleteStoredFile(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        // Veritabanında yalnızca sunucunun oluşturduğu
        // dosya adlarını bekliyoruz.
        if (Path.GetFileName(name) != name)
            return;

        string path = Path.Combine(_storagePath, name);

        if (File.Exists(path))
            File.Delete(path);
    }

    public async Task<IsletmeAyarlariResponse> UploadLogoAsync(
        IFormFile file)
    {
        var item = await _repository.GetOrCreateAsync();

        string newFile = await StoreFileAsync(file, true);
        string? oldFile = item.LogoUrl;

        try
        {
            item.LogoUrl = newFile;
            item.UpdatedAt = DateTime.UtcNow;

            await _repository.SaveAsync();
        }
        catch
        {
            item.LogoUrl = oldFile;
            DeleteStoredFile(newFile);
            throw;
        }

        DeleteStoredFile(oldFile);

        return ToResponse(item);
    }

    public async Task<IsletmeAyarlariResponse> UploadTursabAsync(
        IFormFile file)
    {
        var item = await _repository.GetOrCreateAsync();

        if (!item.TursabUyeligiVar)
        {
            throw new ArgumentException(
                "Önce TÜRSAB üyelik bilgisini etkinleştirin.");
        }

        string newFile = await StoreFileAsync(file, false);
        string? oldFile = item.TursabBelgeUrl;

        try
        {
            item.TursabBelgeUrl = newFile;
            item.UpdatedAt = DateTime.UtcNow;

            await _repository.SaveAsync();
        }
        catch
        {
            item.TursabBelgeUrl = oldFile;
            DeleteStoredFile(newFile);
            throw;
        }

        DeleteStoredFile(oldFile);

        return ToResponse(item);
    }

    public async Task<bool> DeleteLogoAsync()
    {
        var item = await _repository.GetOrCreateAsync();

        if (item.LogoUrl == null)
            return false;

        string oldFile = item.LogoUrl;

        item.LogoUrl = null;
        item.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync();
        DeleteStoredFile(oldFile);

        return true;
    }

    public async Task<bool> DeleteTursabAsync()
    {
        var item = await _repository.GetOrCreateAsync();

        if (item.TursabBelgeUrl == null)
            return false;

        string oldFile = item.TursabBelgeUrl;

        item.TursabBelgeUrl = null;
        item.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveAsync();
        DeleteStoredFile(oldFile);

        return true;
    }

    public async Task<(byte[] Data, string ContentType)?>
        GetFileAsync(bool isLogo)
    {
        var item = await _repository.GetOrCreateAsync();

        string? name = isLogo
            ? item.LogoUrl
            : item.TursabBelgeUrl;

        if (string.IsNullOrWhiteSpace(name))
            return null;

        if (Path.GetFileName(name) != name)
            return null;

        string path = Path.Combine(_storagePath, name);

        if (!File.Exists(path))
            return null;

        byte[] data = await File.ReadAllBytesAsync(path);

        string contentType = ContentTypeFor(
            Path.GetExtension(name).ToLowerInvariant());

        return (data, contentType);
    }
}
