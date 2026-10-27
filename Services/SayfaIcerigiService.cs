
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class SayfaIcerigiService
{
    private readonly AppDbContext _context;
    private readonly HtmlContentService _htmlService;

    private static readonly Dictionary<string, string> Pages = new()
    {
        ["hakkimizda"] = "Hakkımızda",
        ["kiralama-sozlesmesi"] = "Kiralama Sözleşmesi",
        ["musteri-aydinlatma-metni"] = "Müşteri Aydınlatma Metni",
        ["gizlilik-politikasi"] = "Gizlilik Politikası",
        ["cerez-politikasi"] = "Çerez Politikası"
    };

    public SayfaIcerigiService(
        AppDbContext context,
        HtmlContentService htmlService)
    {
        _context = context;
        _htmlService = htmlService;
    }

    public async Task<List<SayfaIcerigi>> GetAllAsync()
    {
        var existing = await _context.SayfaIcerikleri
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Slug);

        return Pages.Select(page =>
        {
            if (existing.TryGetValue(page.Key, out var item))
                return item;

            return new SayfaIcerigi
            {
                Slug = page.Key,
                Baslik = page.Value
            };
        }).ToList();
    }

    public async Task<SayfaIcerigi?> GetBySlugAsync(string slug)
    {
        if (!Pages.ContainsKey(slug))
            return null;

        return await _context.SayfaIcerikleri
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug)
            ?? new SayfaIcerigi
            {
                Slug = slug,
                Baslik = Pages[slug]
            };
    }

    public async Task<SayfaIcerigi> UpdateAsync(
        string slug,
        SayfaIcerigiUpdateRequest request)
    {
        if (!Pages.ContainsKey(slug))
            throw new KeyNotFoundException(
                "Geçersiz sayfa türü."
            );

        var title = request.Baslik.Trim();

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Başlık zorunludur.");

        var safeHtml = _htmlService.Sanitize(
            request.IcerikHtml
        );

        if (string.IsNullOrWhiteSpace(safeHtml))
            throw new ArgumentException("İçerik boş olamaz.");

        var entity = await _context.SayfaIcerikleri
            .FirstOrDefaultAsync(x => x.Slug == slug);

        if (entity == null)
        {
            entity = new SayfaIcerigi { Slug = slug };
            _context.SayfaIcerikleri.Add(entity);
        }

        entity.Baslik = title;
        entity.IcerikHtml = safeHtml;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return entity;
    }
}
