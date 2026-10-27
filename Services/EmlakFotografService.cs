
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class EmlakFotografService
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private const int MaxFileCount = 99;

    private readonly EmlakFotografRepository _repository;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public EmlakFotografService(
        EmlakFotografRepository repository,
        AppDbContext context,
        IWebHostEnvironment environment)
    {
        _repository = repository;
        _context = context;
        _environment = environment;
    }

    private string GetEmlakFolder(int emlakId)
    {
        return Path.Combine(
            _environment.ContentRootPath,
            "wwwroot",
            "uploads",
            "emlaklar",
            emlakId.ToString()
        );
    }

    private string GetPhysicalPath(EmlakFotograf fotograf)
    {
        // Veritabanındaki yolun sadece dosya adı kullanılıyor.
        return Path.Combine(
            GetEmlakFolder(fotograf.EmlakId),
            Path.GetFileName(fotograf.DosyaYolu)
        );
    }

    private static EmlakFotografResponse ToResponse(
        EmlakFotograf f)
    {
        return new EmlakFotografResponse
        {
            Id = f.Id,
            EmlakId = f.EmlakId,
            DosyaYolu = f.DosyaYolu,
            Sira = f.Sira,
            KapakMi = f.KapakMi,
            CreatedAt = f.CreatedAt
        };
    }

    private async Task EnsureEmlakExistsAsync(int emlakId)
    {
        if (!await _repository.EmlakExistsAsync(emlakId))
        {
            throw new KeyNotFoundException(
                "Emlak bulunamadı."
            );
        }
    }

    private static async Task<string> DetectExtensionAsync(
        IFormFile file)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
        {
            throw new ArgumentException(
                "Fotoğraf boyutu 0 ile 10 MB arasında olmalıdır."
            );
        }

        byte[] header = new byte[12];

        await using var stream = file.OpenReadStream();

        int count = await stream.ReadAsync(
            header.AsMemory(0, header.Length)
        );

        if (count >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF)
        {
            return ".jpg";
        }

        if (count >= 8 &&
            header.AsSpan(0, 8).SequenceEqual(
                new byte[]
                {
                    137, 80, 78, 71, 13, 10, 26, 10
                }))
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
            "Yalnızca PNG, JPEG ve WebP fotoğrafları yüklenebilir."
        );
    }

    public async Task<List<EmlakFotografResponse>> GetAsync(
        int emlakId)
    {
        await EnsureEmlakExistsAsync(emlakId);

        var fotograflar = await _repository
            .GetByEmlakIdAsync(emlakId);

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
                "Tek seferde 1-99 fotoğraf yükleyebilirsiniz."
            );
        }

        // Tüm dosyalar kaydedilmeden önce doğrulanır.
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

        var folder = GetEmlakFolder(emlakId);
        Directory.CreateDirectory(folder);

        var savedFiles = new List<string>();
        var newPhotos = new List<EmlakFotograf>();

        try
        {
            foreach (var (file, extension) in validated)
            {
                var fileName =
                    $"{Guid.NewGuid():N}{extension}";

                var fullPath = Path.Combine(folder, fileName);

                await using (var stream = new FileStream(
                    fullPath,
                    FileMode.CreateNew,
                    FileAccess.Write))
                {
                    await file.CopyToAsync(stream);
                }

                savedFiles.Add(fullPath);

                newPhotos.Add(new EmlakFotograf
                {
                    EmlakId = emlakId,
                    DosyaYolu =
                        $"/uploads/emlaklar/{emlakId}/{fileName}",
                    Sira = nextOrder++,
                    KapakMi = !hasCover && newPhotos.Count == 0,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _repository.AddRange(newPhotos);
            await _repository.SaveChangesAsync();

            return newPhotos.Select(ToResponse).ToList();
        }
        catch
        {
            // Veritabanı kaydı başarısız olursa
            // yeni oluşturulan dosyaları temizle.
            foreach (var path in savedFiles)
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch (IOException)
                {
                    // Temizlenemeyen dosyalar loglanabilir.
                }
            }

            throw;
        }
    }

    public async Task DeleteAsync(
        int emlakId,
        int fotografId)
    {
        await EnsureEmlakExistsAsync(emlakId);

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var allPhotos = await _repository.GetTrackedAsync(emlakId);

        var photo = allPhotos.FirstOrDefault(
            f => f.Id == fotografId
        );

        if (photo == null)
        {
            throw new KeyNotFoundException(
                "Fotoğraf bulunamadı."
            );
        }

        bool wasCover = photo.KapakMi;
        var physicalPath = GetPhysicalPath(photo);

        _repository.Delete(photo);

        // Önce silme işlemini SQL'e uygula.
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

        int order = 0;

        foreach (var item in allPhotos
            .Where(f => f.Id != fotografId)
            .OrderBy(f => f.Sira)
            .ThenBy(f => f.Id))
        {
            item.Sira = order++;
        }

        await _repository.SaveChangesAsync();
        await transaction.CommitAsync();

        // Veritabanı başarılı olduktan sonra dosyayı sil.
        try
        {
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
        }
        catch (IOException)
        {
            // Üretimde logger ile kaydedilmeli.
        }
    }

    public async Task SetCoverAsync(
        int emlakId,
        int fotografId)
    {
        await EnsureEmlakExistsAsync(emlakId);

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var photos = await _repository.GetTrackedAsync(emlakId);

        var selected = photos.FirstOrDefault(
            f => f.Id == fotografId
        );

        if (selected == null)
        {
            throw new KeyNotFoundException(
                "Fotoğraf bulunamadı."
            );
        }

        if (selected.KapakMi)
            return;

        // Unique filtered index nedeniyle önce
        // mevcut kapak kaldırılıp SQL'e yazılır.
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
                "Fotoğraf listesi eksik veya geçersiz."
            );
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

    private static int fotograflarCount(
        List<EmlakFotograf> photos)
    {
        return photos.Count;
    }
}
