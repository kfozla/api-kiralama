using System.Data;
using api_kiralama.Data;
using api_kiralama.Models;
using api_kiralama.Repositories;
using api_kiralama.Dtos;
using Microsoft.EntityFrameworkCore;

namespace api_kiralama.Services;

public class RezervasyonService
{
    private readonly AppDbContext _context;
    private readonly RezervasyonRepository _repository;
    private readonly EmlakFiyatRepository _fiyatRepository;
    private readonly EmlakTakvimLock _takvimLock;

    public RezervasyonService(
        AppDbContext context,
        RezervasyonRepository repository,
        EmlakFiyatRepository fiyatRepository,
        EmlakTakvimLock takvimLock)
    {
        _context = context;
        _repository = repository;
        _fiyatRepository = fiyatRepository;
        _takvimLock = takvimLock;
    }

    public Task<List<Rezervasyon>> GetAllAsync()
        => _repository.GetAllAsync();

    public Task<Rezervasyon?> GetByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    public Task<List<Rezervasyon>> GetByEmlakIdAsync(int emlakId)
        => _repository.GetByEmlakIdAsync(emlakId);
    
    
    public async Task<PagedResponse<RezervasyonListResponse>> GetPagedAsync(
        RezervasyonPagedRequest request)
    {
        if (request.GirisBaslangic.HasValue &&
            request.GirisBitis.HasValue &&
            request.GirisBaslangic.Value >
            request.GirisBitis.Value)
        {
            throw new ArgumentException(
                "Giriş başlangıç tarihi, bitiş tarihinden büyük olamaz."
            );
        }

        if (request.Durum.HasValue &&
            !Enum.IsDefined(
                typeof(RezervasyonDurumu),
                request.Durum.Value))
        {
            throw new ArgumentException(
                "Geçersiz rezervasyon durumu."
            );
        }

        return await _repository.GetPagedAsync(request);
    }
    
    public async Task<RezervasyonStatsResponse> GetStatsAsync()
    {
        return await _repository.GetStatsAsync();
    }
    
    
    public async Task<RezervasyonTakvimResponse> GetTakvimAsync(
        RezervasyonTakvimRequest request)
    {
        if (request.Year < 2000 || request.Year > 2100 ||
            request.Month < 1 || request.Month > 12 ||
            request.Today == default)
        {
            throw new ArgumentException(
                "Geçersiz takvim parametreleri."
            );
        }

        return await _repository.GetTakvimAsync(
            request
        );
    }




    private static void ValidateDates(
        DateOnly giris, DateOnly cikis)
    {
        if (giris >= cikis)
            throw new ArgumentException(
                "Çıkış tarihi giriş tarihinden sonra olmalı.");

        if (giris < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException(
                "Geçmiş tarihe rezervasyon yapılamaz.");
    }
    private static void ValidateRequest(RezervasyonRequest request)
    {
        ValidateDates(request.GirisTarihi, request.CikisTarihi);

        if (request.KisiSayisi <= 0)
            throw new ArgumentException("Kişi sayısı geçersiz.");

        if (string.IsNullOrWhiteSpace(request.MusteriAd) ||
            string.IsNullOrWhiteSpace(request.MusteriSoyad) ||
            string.IsNullOrWhiteSpace(request.Telefon))
        {
            throw new ArgumentException(
                "Müşteri adı, soyadı ve telefon zorunludur.");
        }
    }

    public async Task<decimal> CalculatePriceAsync(
        int emlakId,
        DateOnly giris,
        DateOnly cikis)
    {
        if (giris >= cikis)
            throw new ArgumentException("Tarih aralığı geçersiz.");

        var fiyatlar = await _fiyatRepository.GetByDateRangeAsync(
            emlakId, giris, cikis);

        decimal toplam = 0;

        for (var gun = giris; gun < cikis; gun = gun.AddDays(1))
        {
            var gunFiyatlari = fiyatlar
                .Where(f =>
                    f.BaslangicTarihi <= gun &&
                    f.BitisTarihi > gun)
                .ToList();

            if (gunFiyatlari.Count != 1)
                throw new InvalidOperationException(
                    $"{gun:yyyy-MM-dd} için geçerli tek bir fiyat bulunamadı.");

            toplam += gunFiyatlari[0].GecelikFiyat;
        }

        return toplam;
    }
    
    private async Task<bool> HasBlockConflictAsync(
        int emlakId,
        DateOnly giris,
        DateOnly cikis,
        int? excludeRezervasyonId = null)
    {
        return await _context.Set<EmlakBlokeTarih>()
            .AnyAsync(b =>
                b.EmlakId == emlakId &&
                (!excludeRezervasyonId.HasValue ||
                 b.RezervasyonId != excludeRezervasyonId.Value) &&
                b.BaslangicTarihi < cikis &&
                giris < b.BitisTarihi);
    }


    // Temizlik sadece belirlenen gece sınırının ALTINDA uygulanır.
    // Sınır 0 ise temizlik bedeli otomatik uygulanmaz.
    private static decimal CalculateCleaningFee(Emlak emlak, int geceSayisi)
    {
        return emlak.MinimumKonaklamaTemizlik > 0 &&
               geceSayisi < emlak.MinimumKonaklamaTemizlik
            ? emlak.TemizlikUcreti
            : 0m;
    }

    private static int ValidateMinimumNights(
        Emlak emlak, DateOnly giris, DateOnly cikis)
    {
        int geceSayisi = cikis.DayNumber - giris.DayNumber;
        if (geceSayisi < emlak.MinimumKonaklamaSuresi)
        {
            throw new ArgumentException(
                $"Bu emlak için minimum konaklama süresi " +
                $"{emlak.MinimumKonaklamaSuresi} gecedir.");
        }

        return geceSayisi;
    }

    public async Task<Rezervasyon> AddAsync(RezervasyonRequest request)
    {
        ValidateRequest(request);

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        await _takvimLock.LockAsync(request.EmlakId);

        var emlak = await _context.Emlaklar
            .FirstOrDefaultAsync(e =>
                e.Id == request.EmlakId && !e.IsDeleted);

        if (emlak == null)
            throw new ArgumentException("Emlak bulunamadı.");

        int geceSayisi = ValidateMinimumNights(
            emlak, request.GirisTarihi, request.CikisTarihi);

        if (request.KisiSayisi > emlak.Kapasite)
            throw new ArgumentException(
                "Kişi sayısı emlak kapasitesini aşıyor.");

        bool conflict = await _repository.HasConflictAsync(
            request.EmlakId,
            request.GirisTarihi,
            request.CikisTarihi);

        if (conflict)
            throw new InvalidOperationException(
                "Bu tarih aralığında villa müsait değil.");

        decimal konaklamaTutari = await CalculatePriceAsync(
            request.EmlakId,
            request.GirisTarihi,
            request.CikisTarihi);

        decimal temizlikUcreti = CalculateCleaningFee(emlak, geceSayisi);
        

        var rezervasyon = new Rezervasyon
        {
            EmlakId = request.EmlakId,
            MusteriAd = request.MusteriAd.Trim(),
            MusteriSoyad = request.MusteriSoyad.Trim(),
            Telefon = request.Telefon.Trim(),
            Email = request.Email,
            GirisTarihi = request.GirisTarihi,
            CikisTarihi = request.CikisTarihi,
            KisiSayisi = request.KisiSayisi,
            ToplamFiyat = konaklamaTutari,
            TemizlikUcreti = temizlikUcreti,
            HasarDepozitosu = emlak.HasarDepozitosu,
            Durum = RezervasyonDurumu.Beklemede,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(rezervasyon);
        await transaction.CommitAsync();

        return rezervasyon;
    }
    
   
public async Task<bool> UpdateAsync(
    int id, RezervasyonRequest request)
{
    ValidateRequest(request);

    var current = await _context.Rezervasyonlar
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.Id == id);

    if (current == null)
        return false;

    if (current.EmlakId != request.EmlakId)
        throw new ArgumentException(
            "Rezervasyonun emlak ID'si değiştirilemez.");

    await using var transaction = await _context.Database
        .BeginTransactionAsync(IsolationLevel.Serializable);

    await _takvimLock.LockAsync(current.EmlakId);

    var rezervasyon = await _context.Rezervasyonlar
        .FirstOrDefaultAsync(r => r.Id == id);

    if (rezervasyon == null)
        return false;

    if (rezervasyon.EmlakId != current.EmlakId)
        throw new InvalidOperationException(
            "Rezervasyonun emlak bilgisi değişmiş.");

    if (rezervasyon.Durum == RezervasyonDurumu.IptalEdildi ||
        rezervasyon.Durum == RezervasyonDurumu.Tamamlandi)
    {
        throw new InvalidOperationException(
            "İptal edilmiş veya tamamlanmış rezervasyon güncellenemez.");
    }

    var emlak = await _context.Emlaklar
        .FirstOrDefaultAsync(e =>
            e.Id == request.EmlakId && !e.IsDeleted);

    if (emlak == null)
        throw new ArgumentException("Emlak bulunamadı.");

    // Minimum konaklama süresi kontrolü
    int geceSayisi = ValidateMinimumNights(
        emlak,
        request.GirisTarihi,
        request.CikisTarihi);

    // Kapasite kontrolü
    if (request.KisiSayisi > emlak.Kapasite)
        throw new ArgumentException(
            "Kişi sayısı emlak kapasitesini aşıyor.");

    // Tarihlerin değişip değişmediğini kontrol et
    bool tarihDegisti =
        rezervasyon.GirisTarihi != request.GirisTarihi ||
        rezervasyon.CikisTarihi != request.CikisTarihi;

    if (tarihDegisti)
    {
        // Manuel ve diğer rezervasyon kaynaklı blokeler
        bool blokeCakismasi = await HasBlockConflictAsync(
            request.EmlakId,
            request.GirisTarihi,
            request.CikisTarihi,
            id);

        if (blokeCakismasi)
        {
            throw new InvalidOperationException(
                "Yeni tarihlerde bloke bulunuyor.");
        }

        // Diğer rezervasyonlarla çakışma kontrolü
        bool rezervasyonCakismasi =
            await _repository.HasConflictAsync(
                request.EmlakId,
                request.GirisTarihi,
                request.CikisTarihi,
                id);

        if (rezervasyonCakismasi)
        {
            throw new InvalidOperationException(
                "Yeni tarihlerde başka rezervasyon bulunuyor.");
        }

        // Yalnızca konaklama bedelini hesapla
        decimal konaklamaTutari = await CalculatePriceAsync(
            request.EmlakId,
            request.GirisTarihi,
            request.CikisTarihi);

        // Koşullu temizlik ücretini hesapla
        decimal temizlikUcreti =
            CalculateCleaningFee(emlak, geceSayisi);

        // ToplamFiyat sadece konaklama tutarıdır
        rezervasyon.ToplamFiyat = konaklamaTutari;

        // Temizlik ücreti ayrı kaydedilir
        rezervasyon.TemizlikUcreti = temizlikUcreti;

        // HasarDepozitosu değiştirilmez.
        // Rezervasyon oluşturulurken kaydedilen değer korunur.

        // Onaylı rezervasyonun bloke tarihlerini eşitle
        if (rezervasyon.Durum == RezervasyonDurumu.Onaylandi)
        {
            var bloke = await _context.Set<EmlakBlokeTarih>()
                .FirstOrDefaultAsync(
                    b => b.RezervasyonId == rezervasyon.Id);

            if (bloke == null)
            {
                bloke = new EmlakBlokeTarih
                {
                    EmlakId = rezervasyon.EmlakId,
                    RezervasyonId = rezervasyon.Id,
                    BaslangicTarihi = request.GirisTarihi,
                    BitisTarihi = request.CikisTarihi,
                    Aciklama = $"Rezervasyon #{rezervasyon.Id}"
                };

                await _context.Set<EmlakBlokeTarih>()
                    .AddAsync(bloke);
            }
            else
            {
                bloke.BaslangicTarihi = request.GirisTarihi;
                bloke.BitisTarihi = request.CikisTarihi;
            }
        }
    }

    // Müşteri bilgilerini güncelle
    rezervasyon.MusteriAd = request.MusteriAd.Trim();
    rezervasyon.MusteriSoyad = request.MusteriSoyad.Trim();
    rezervasyon.Telefon = request.Telefon.Trim();
    rezervasyon.Email = request.Email;

    // Konaklama bilgilerini güncelle
    rezervasyon.GirisTarihi = request.GirisTarihi;
    rezervasyon.CikisTarihi = request.CikisTarihi;
    rezervasyon.KisiSayisi = request.KisiSayisi;

    rezervasyon.UpdatedAt = DateTime.UtcNow;

    await _repository.UpdateAsync();
    await transaction.CommitAsync();

    return true;
}


    
public async Task<bool> ChangeStatusAsync(
    int id,
    RezervasyonDurumu durum)
{
    if (!Enum.IsDefined(typeof(RezervasyonDurumu), durum))
        throw new ArgumentException(
            "Geçersiz rezervasyon durumu.");

    var mevcut = await _context.Rezervasyonlar
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.Id == id);

    if (mevcut == null)
        return false;

    await using var transaction = await _context.Database
        .BeginTransactionAsync(IsolationLevel.Serializable);

    await _takvimLock.LockAsync(mevcut.EmlakId);

    var rezervasyon = await _context.Rezervasyonlar
        .FirstOrDefaultAsync(r => r.Id == id);

    if (rezervasyon == null)
        return false;

    // Başka bir işlem sırasında emlak değiştirilmişse
    if (rezervasyon.EmlakId != mevcut.EmlakId)
        throw new InvalidOperationException(
            "Rezervasyonun emlak bilgisi değişmiş.");

    if (rezervasyon.Durum == durum)
    {
        await transaction.CommitAsync();
        return true;
    }

    if (rezervasyon.Durum == RezervasyonDurumu.Tamamlandi)
        throw new InvalidOperationException(
            "Tamamlanan rezervasyonun durumu değiştirilemez.");

    if (rezervasyon.Durum == RezervasyonDurumu.IptalEdildi)
        throw new InvalidOperationException(
            "İptal edilen rezervasyon tekrar aktif hale getirilemez.");

    if (durum == RezervasyonDurumu.Beklemede)
        throw new InvalidOperationException(
            "Rezervasyon beklemede durumuna geri alınamaz.");

    if (durum == RezervasyonDurumu.Tamamlandi &&
        rezervasyon.Durum != RezervasyonDurumu.Onaylandi)
    {
        throw new InvalidOperationException(
            "Yalnızca onaylı rezervasyon tamamlanabilir.");
    }

    var mevcutBloke = await _context.Set<EmlakBlokeTarih>()
        .FirstOrDefaultAsync(b => b.RezervasyonId == id);

    // 1. REZERVASYON ONAYLANDI
    if (durum == RezervasyonDurumu.Onaylandi)
    {
        bool blokeCakismasi = await HasBlockConflictAsync(
            rezervasyon.EmlakId,
            rezervasyon.GirisTarihi,
            rezervasyon.CikisTarihi,
            id);

        if (blokeCakismasi)
            throw new InvalidOperationException(
                "Bu tarih aralığı bloke edilmiş. " +
                "Rezervasyon onaylanamaz.");

        // Diğer aktif rezervasyonlarla çakışma kontrolü.
        // HasConflictAsync metodunun kendi rezervasyonunu
        // hariç tuttuğunu doğrulamalıyız.
        bool rezervasyonCakismasi =
            await _repository.HasConflictAsync(
                rezervasyon.EmlakId,
                rezervasyon.GirisTarihi,
                rezervasyon.CikisTarihi,
                id);

        if (rezervasyonCakismasi)
            throw new InvalidOperationException(
                "Bu tarihlerde başka bir rezervasyon bulunuyor.");

        if (mevcutBloke == null)
        {
            var bloke = new EmlakBlokeTarih
            {
                EmlakId = rezervasyon.EmlakId,
                RezervasyonId = rezervasyon.Id,
                BaslangicTarihi = rezervasyon.GirisTarihi,
                BitisTarihi = rezervasyon.CikisTarihi,
                Aciklama = $"Rezervasyon #{rezervasyon.Id}"
            };

            await _context.Set<EmlakBlokeTarih>()
                .AddAsync(bloke);
        }
        else
        {
            // Normalde bu dala girilmez; ancak eski bir
            // kayıt varsa doğru tarihlerle eşitle.
            mevcutBloke.BaslangicTarihi =
                rezervasyon.GirisTarihi;

            mevcutBloke.BitisTarihi =
                rezervasyon.CikisTarihi;
        }
    }

    // 2. REZERVASYON İPTAL EDİLDİ
    if (durum == RezervasyonDurumu.IptalEdildi)
    {
        if (mevcutBloke != null)
        {
            _context.Set<EmlakBlokeTarih>()
                .Remove(mevcutBloke);
        }
    }

    // 3. REZERVASYON TAMAMLANDI
    // Bloke kaydını koruyoruz; ek işlem yok.

    rezervasyon.Durum = durum;
    rezervasyon.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    await transaction.CommitAsync();

    return true;
}


    public async Task<bool> CheckAvailabilityAsync(
        int emlakId,
        DateOnly giris,
        DateOnly cikis)
    {
        // Tarih doğrulaması
        ValidateDates(giris, cikis);

        // Emlak mevcut mu, silinmiş mi ve minimum konaklamaya uyuyor mu?
        var emlak = await _context.Emlaklar
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == emlakId && !e.IsDeleted);

        if (emlak == null)
            throw new ArgumentException("Emlak bulunamadı.");

        if (cikis.DayNumber - giris.DayNumber < emlak.MinimumKonaklamaSuresi)
            return false;

        // Rezervasyon ve manuel bloke kontrolü
        bool conflict = await _repository.HasConflictAsync(
            emlakId,
            giris,
            cikis
        );

        if (conflict)
            return false;

        // Seçilen tüm geceler için fiyat kontrolü
        try
        {
            await CalculatePriceAsync(emlakId, giris, cikis);

            return true;
        }
        catch (InvalidOperationException)
        {
            // Fiyat tanımlanmamış veya çakışan fiyatlar var
            return false;
        }
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var current = await _context.Rezervasyonlar
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (current == null)
            return false;

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        await _takvimLock.LockAsync(current.EmlakId);

        // Kilitten sonra kaydı yeniden alıyoruz.
        var rezervasyon = await _repository.GetByIdAsync(id);

        if (rezervasyon == null)
            return false;

        if (rezervasyon.EmlakId != current.EmlakId)
        {
            throw new InvalidOperationException(
                "Rezervasyonun emlak bilgisi değişmiş.");
        }

        // Yalnızca bekleyen rezervasyonlar silinebilir.
        if (rezervasyon.Durum != RezervasyonDurumu.Beklemede)
        {
            throw new InvalidOperationException(
                "Yalnızca bekleyen rezervasyonlar silinebilir. " +
                "Onaylı rezervasyonları iptal etmelisiniz.");
        }

        // Bekleyen rezervasyonun otomatik blokesi olmamalı.
        // Veri tutarsızlığı varsa silmeyi engelliyoruz.
        bool hasBlock = await _context.EmlakBlokeTarihler
            .AnyAsync(b => b.RezervasyonId == rezervasyon.Id);

        if (hasBlock)
        {
            throw new InvalidOperationException(
                "Bu rezervasyona bağlı bloke kaydı bulunuyor. " +
                "Rezervasyon silinemez.");
        }

        await _repository.DeleteAsync(rezervasyon);

        await transaction.CommitAsync();

        return true;
    }

}