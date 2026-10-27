using api_kiralama.Data;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class EmlakTakvimLock
{
    private readonly AppDbContext _context;

    public EmlakTakvimLock(AppDbContext context)
    {
        _context = context;
    }

    public async Task LockAsync(int emlakId)
    {
        // SQL Server üzerinde mevcut emlak satırına yazma kilidi alır.
        // Mutlaka açık bir transaction içerisinde çağrılmalıdır.

        int affected = await _context.Database
            .ExecuteSqlInterpolatedAsync(
                $"UPDATE Emlaklar SET UpdatedAt = UpdatedAt WHERE Id = {emlakId} AND IsDeleted = 0");

        if (affected == 0)
            throw new ArgumentException("Emlak bulunamadı.");
    }
}