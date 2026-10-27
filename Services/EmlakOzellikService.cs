using api_kiralama.Models;
using api_kiralama.Repositories;
using api_kiralama.Dtos;

namespace api_kiralama.Services;

public class EmlakOzellikService
{
    private readonly EmlakOzellikRepository _repository;
    private readonly IWebHostEnvironment _environment;

    public EmlakOzellikService(EmlakOzellikRepository repository, IWebHostEnvironment environment)
    {
        _repository = repository;
        _environment = environment;
    }

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
        string ad = request.OzellikAdi.Trim();

        if (string.IsNullOrWhiteSpace(ad))
            throw new ArgumentException("Özellik adı boş olamaz.");

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
        int id, EmlakOzellikRequest request)
    {
        var ozellik = await _repository.GetByIdAsync(id);

        if (ozellik == null)
            return false;

        string ad = request.OzellikAdi.Trim();

        if (string.IsNullOrWhiteSpace(ad))
            throw new ArgumentException("Özellik adı boş olamaz.");

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

        await _repository.DeleteAsync(ozellik);

        return true;
    }
    
    
private const long MaxLogoSize = 5 * 1024 * 1024;

private const string LogoFolder = "emlak-ozellikler";

public async Task<string> UploadLogoAsync(
    int ozellikId,
    IFormFile file)
{
    var ozellik = await _repository.GetByIdForLogoAsync(ozellikId);

    if (ozellik == null)
        throw new KeyNotFoundException("Emlak özelliği bulunamadı.");

    if (file == null || file.Length == 0)
        throw new ArgumentException("Logo dosyası seçilmelidir.");

    if (file.Length > MaxLogoSize)
        throw new ArgumentException("Logo en fazla 5 MB olabilir.");

    // Dosyanın gerçek formatını kontrol et.
    byte[] header = new byte[12];

    await using (var input = file.OpenReadStream())
    {
        int offset = 0;

        while (offset < header.Length)
        {
            int count = await input.ReadAsync(
                header.AsMemory(offset, header.Length - offset));

            if (count == 0)
                break;

            offset += count;
        }

        if (offset < 12)
            throw new ArgumentException("Geçersiz görsel dosyası.");
    }

    bool isPng =
        header.Take(8).SequenceEqual(
            new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

    bool isJpeg =
        header[0] == 0xFF &&
        header[1] == 0xD8 &&
        header[2] == 0xFF;

    bool isWebp =
        System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
        System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP";

    if (!isPng && !isJpeg && !isWebp)
        throw new ArgumentException(
            "Yalnızca PNG, JPG ve WebP dosyaları yüklenebilir.");

    string extension = isPng
        ? ".png"
        : isJpeg
            ? ".jpg"
            : ".webp";

    string webRoot = _environment.WebRootPath
        ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

    string folder = Path.Combine(
        webRoot,
        "uploads",
        LogoFolder);

    Directory.CreateDirectory(folder);

    string fileName = $"{Guid.NewGuid():N}{extension}";

    string filePath = Path.Combine(folder, fileName);

    string logoUrl = $"/uploads/{LogoFolder}/{fileName}";

    await using (var stream = new FileStream(
        filePath,
        FileMode.CreateNew))
    {
        await file.CopyToAsync(stream);
    }

    string? oldLogoUrl = ozellik.LogoUrl;

    ozellik.LogoUrl = logoUrl;

    try
    {
        await _repository.SaveChangesAsync();
    }
    catch
    {
        if (File.Exists(filePath))
            File.Delete(filePath);

        throw;
    }

    // Veritabanı başarıyla güncellendikten sonra eski dosyayı temizle.
    DeleteLocalLogo(oldLogoUrl);

    return logoUrl;
}

public async Task<bool> DeleteLogoAsync(int ozellikId)
{
    var ozellik = await _repository.GetByIdForLogoAsync(ozellikId);

    if (ozellik == null)
        return false;

    string? oldLogoUrl = ozellik.LogoUrl;

    ozellik.LogoUrl = null;

    await _repository.SaveChangesAsync();

    DeleteLocalLogo(oldLogoUrl);

    return true;
}

private void DeleteLocalLogo(string? logoUrl)
{
    if (string.IsNullOrWhiteSpace(logoUrl))
        return;

    string prefix = $"/uploads/{LogoFolder}/";

    if (!logoUrl.StartsWith(prefix, StringComparison.Ordinal))
        return;

    string fileName = logoUrl.Substring(prefix.Length);

    // Yalnızca doğrudan bu klasördeki dosyalar silinebilir.
    if (fileName != Path.GetFileName(fileName))
        return;

    string webRoot = _environment.WebRootPath
        ?? Path.Combine(_environment.ContentRootPath, "wwwroot");

    string filePath = Path.Combine(
        webRoot,
        "uploads",
        LogoFolder,
        fileName);

    if (File.Exists(filePath))
        File.Delete(filePath);
}

}