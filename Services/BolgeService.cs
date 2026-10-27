using api_kiralama.Models;
using api_kiralama.Repositories;
using api_kiralama.Dtos;

namespace api_kiralama.Services;

public class BolgeService
{
    private readonly BolgeRepository _repository;

    public BolgeService(BolgeRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Bolge>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Bolge?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<List<Bolge>> GetAltBolgelerAsync(int? parentId)
    {
        return await _repository.GetAltBolgelerAsync(parentId);
    }

    private async Task ValidateParentAsync(
        int? parentId, int? currentId = null)
    {
        if (!parentId.HasValue)
            return;

        if (parentId.Value <= 0)
            throw new ArgumentException("Geçersiz üst bölge ID.");

        if (currentId == parentId)
            throw new ArgumentException(
                "Bir bölge kendisinin üst bölgesi olamaz.");

        var visited = new HashSet<int>();
        int? nextId = parentId;

        while (nextId.HasValue)
        {
            if (!visited.Add(nextId.Value))
                throw new InvalidOperationException(
                    "Bölge hiyerarşisinde döngü tespit edildi.");

            if (currentId == nextId)
                throw new ArgumentException(
                    "Bir bölge kendi alt bölgesinin altına taşınamaz.");

            var parent = await _repository.GetByIdAsync(nextId.Value);

            if (parent == null)
                throw new ArgumentException("Üst bölge bulunamadı.");

            nextId = parent.ParentBolgeId;
        }
    }

    public async Task<Bolge> AddAsync(BolgeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Ad))
            throw new ArgumentException("Bölge adı boş olamaz.");

        await ValidateParentAsync(request.ParentBolgeId);

        var bolge = new Bolge
        {
            Ad = request.Ad.Trim(),
            ParentBolgeId = request.ParentBolgeId
        };

        return await _repository.AddAsync(bolge);
    }

    public async Task<bool> UpdateAsync(int id, BolgeRequest request)
    {
        var bolge = await _repository.GetByIdAsync(id);

        if (bolge == null)
            return false;

        if (string.IsNullOrWhiteSpace(request.Ad))
            throw new ArgumentException("Bölge adı boş olamaz.");

        await ValidateParentAsync(request.ParentBolgeId, id);

        bolge.Ad = request.Ad.Trim();
        bolge.ParentBolgeId = request.ParentBolgeId;

        await _repository.UpdateAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var bolge = await _repository.GetByIdAsync(id);

        if (bolge == null)
            return false;

        if (await _repository.HasAltBolgeAsync(id))
            throw new InvalidOperationException(
                "Bu bölgenin alt bölgeleri bulunduğu için silinemez.");

        if (await _repository.HasEmlakAsync(id))
            throw new InvalidOperationException(
                "Bu bölgeye bağlı emlaklar bulunduğu için silinemez.");

        await _repository.DeleteAsync(bolge);

        return true;
    }
}