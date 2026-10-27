
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class SikcaSorulanSoruService
{
    private readonly AppDbContext _context;
    private readonly HtmlContentService _htmlService;

    public SikcaSorulanSoruService(
        AppDbContext context,
        HtmlContentService htmlService)
    {
        _context = context;
        _htmlService = htmlService;
    }

    public async Task<List<SikcaSorulanSoru>> GetAllAsync(
        bool includeInactive = false)
    {
        var query = _context.SikcaSorulanSorular
            .AsNoTracking()
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(x => x.AktifMi);
        }

        return await query
            .OrderBy(x => x.Sira)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<SikcaSorulanSoru> CreateAsync(
        SikcaSorulanSoruRequest request)
    {
        var entity = new SikcaSorulanSoru();

        Apply(entity, request);

        _context.SikcaSorulanSorular.Add(entity);

        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<SikcaSorulanSoru?> UpdateAsync(
        int id,
        SikcaSorulanSoruRequest request)
    {
        var entity = await _context.SikcaSorulanSorular
            .FindAsync(id);

        if (entity == null)
            return null;

        Apply(entity, request);

        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.SikcaSorulanSorular
            .FindAsync(id);

        if (entity == null)
            return false;

        _context.SikcaSorulanSorular.Remove(entity);

        await _context.SaveChangesAsync();

        return true;
    }

    private void Apply(
        SikcaSorulanSoru entity,
        SikcaSorulanSoruRequest request)
    {
        var soru = request.Soru.Trim();

        if (string.IsNullOrWhiteSpace(soru))
            throw new ArgumentException("Soru zorunludur.");

        var cevap = _htmlService.Sanitize(
            request.CevapHtml
        );

        if (string.IsNullOrWhiteSpace(cevap))
            throw new ArgumentException("Cevap zorunludur.");

        entity.Soru = soru;
        entity.CevapHtml = cevap;
        entity.Sira = request.Sira;
        entity.AktifMi = request.AktifMi;
    }
}
