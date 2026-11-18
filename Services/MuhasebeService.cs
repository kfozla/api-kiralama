using api_kiralama.Dtos;
using api_kiralama.Repositories;

namespace api_kiralama.Services;

public class MuhasebeService
{
    private readonly MuhasebeRepository _repository;

    public MuhasebeService(MuhasebeRepository repository)
    {
        _repository = repository;
    }

    public async Task<object> GetPagedAsync(
        MuhasebePagedRequest request)
    {
        var (items, totalCount) =
            await _repository.GetPagedAsync(request);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        return new
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
    
    public async Task<MuhasebeOzetResponse> GetOzetAsync(
        MuhasebeOzetRequest request)
    {
        if (request.BaslangicTarihi.HasValue &&
            request.BitisTarihi.HasValue &&
            request.BitisTarihi.Value < request.BaslangicTarihi.Value)
        {
            throw new ArgumentException(
                "Bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        return await _repository.GetOzetAsync(request);
    }
    public async Task<List<MuhasebeAylikGrafikResponse>> GetAylikGrafikAsync(
        MuhasebeOzetRequest request)
    {
        if (request.BaslangicTarihi.HasValue &&
            request.BitisTarihi.HasValue &&
            request.BitisTarihi.Value < request.BaslangicTarihi.Value)
            throw new ArgumentException(
                "Bitiş tarihi başlangıç tarihinden önce olamaz.");

        return await _repository.GetAylikGrafikAsync(request);
    }

}