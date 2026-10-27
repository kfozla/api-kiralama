
using api_kiralama.Models;
using Microsoft.EntityFrameworkCore;
namespace api_kiralama.Data;

public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options){}
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Emlak> Emlaklar { get; set; }
    public DbSet<EmlakOzellik> EmlakOzellikleri { get; set; }
    public DbSet<EmlakSahibi> EmlakSahipleri { get; set; }
    public DbSet<Bolge> Bolgeler { get; set; }
    public DbSet<EmlakFiyat> EmlakFiyatlari { get; set; }
    public DbSet<EmlakBlokeTarih> EmlakBlokeTarihler { get; set; }
    public DbSet<Rezervasyon> Rezervasyonlar { get; set; }
    public DbSet<IsletmeAyarlari> IsletmeAyarlari { get; set; }
    public DbSet<EmlakFotograf> EmlakFotograflari { get; set; }
    
    public DbSet<SayfaIcerigi> SayfaIcerikleri { get; set; }

    public DbSet<SikcaSorulanSoru> SikcaSorulanSorular { get; set; }

    public DbSet<BankaHesabi> BankaHesaplari { get; set; }
    
    public DbSet<AnasayfaBolum> AnasayfaBolumler { get; set; }
    public DbSet<AnasayfaBolumEmlak> AnasayfaBolumEmlaklar { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<EmlakOzellik>()
            .HasIndex(o => new { o.OzellikAdi, o.Kategori })
            .IsUnique();
        
        modelBuilder.Entity<Emlak>()
            .HasMany(e => e.EmlakOzellikleri)
            .WithMany(o => o.Emlaklar)
            .UsingEntity(j => j.ToTable("EmlakOzellikIliskileri"));
        
        // Emlak - Öne çıkan özellikler
        modelBuilder.Entity<Emlak>()
            .HasMany(e => e.OneCıkanOzellikler)
            .WithMany()
            .UsingEntity(j => j.ToTable("EmlakOneCikanOzellikIliskileri"));
        
        modelBuilder.Entity<Emlak>()
            .HasOne<EmlakSahibi>()
            .WithMany()
            .HasForeignKey(e => e.EmlakSahibiId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        // Bölge -> Alt Bölge
        modelBuilder.Entity<Bolge>()
            .HasOne<Bolge>()
            .WithMany()
            .HasForeignKey(b => b.ParentBolgeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Emlak -> Bölge
        modelBuilder.Entity<Emlak>()
            .HasOne<Bolge>()
            .WithMany()
            .HasForeignKey(e => e.BolgeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Bolge>()
            .HasIndex(b => b.ParentBolgeId);
        
        modelBuilder.Entity<Emlak>()
            .OwnsMany(e => e.HavuzBilgileri, builder =>
            {
                builder.ToJson();
            });
        
        modelBuilder.Entity<EmlakFiyat>()
            .Property(f => f.GecelikFiyat)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Rezervasyon>()
            .Property(r => r.ToplamFiyat)
            .HasPrecision(18, 2);
        
        modelBuilder.Entity<Rezervasyon>(entity =>
        {
            entity.HasIndex(r => new
            {
                r.CreatedAt,
                r.Id
            });

            entity.HasIndex(r => new
            {
                r.Durum,
                r.GirisTarihi
            });

            entity.HasIndex(r => r.EmlakId);
        });

        modelBuilder.Entity<EmlakFiyat>()
            .HasOne<Emlak>()
            .WithMany()
            .HasForeignKey(f => f.EmlakId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Rezervasyon>()
            .HasOne<Emlak>()
            .WithMany()
            .HasForeignKey(r => r.EmlakId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmlakBlokeTarih>()
            .HasOne<Emlak>()
            .WithMany()
            .HasForeignKey(b => b.EmlakId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        modelBuilder.Entity<EmlakBlokeTarih>()
            .HasOne<Rezervasyon>()
            .WithMany()
            .HasForeignKey(x => x.RezervasyonId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EmlakBlokeTarih>()
            .HasIndex(x => x.RezervasyonId)
            .IsUnique()
            .HasFilter("[RezervasyonId] IS NOT NULL");
        
        modelBuilder.Entity<IsletmeAyarlari>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.IsletmeAdi)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.YetkiliKisi)
                .HasMaxLength(150);

            entity.Property(x => x.Email)
                .HasMaxLength(200);

            entity.Property(x => x.LogoUrl)
                .HasMaxLength(255);

            entity.Property(x => x.TursabBelgeUrl)
                .HasMaxLength(255);

            entity.Property(x => x.TursabBelgeNo)
                .HasMaxLength(100);
        });
        
        modelBuilder.Entity<EmlakFotograf>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DosyaYolu)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(x => x.EmlakId);

            entity.HasIndex(x => new { x.EmlakId, x.Sira });

            entity.HasOne<Emlak>()
                .WithMany()
                .HasForeignKey(x => x.EmlakId)
                .OnDelete(DeleteBehavior.Cascade);

            // Her emlağın en fazla bir kapak fotoğrafı olabilir.
            entity.HasIndex(x => x.EmlakId)
                .IsUnique()
                .HasFilter("[KapakMi] = 1");
        });
        
        
        modelBuilder.Entity<SayfaIcerigi>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Slug)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => x.Slug)
                .IsUnique();

            entity.Property(x => x.Baslik)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.IcerikHtml)
                .HasColumnType("nvarchar(max)")
                .IsRequired();
        });

        modelBuilder.Entity<SikcaSorulanSoru>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Soru)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.CevapHtml)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            entity.HasIndex(x => x.Sira);
        });

        modelBuilder.Entity<BankaHesabi>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BankaAdi)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.HesapSahibi)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Iban)
                .HasMaxLength(34)
                .IsRequired();

            entity.Property(x => x.SubeAdi)
                .HasMaxLength(150);

            entity.Property(x => x.ParaBirimi)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(x => x.LogoUrl)
                .HasMaxLength(500);

            entity.HasIndex(x => x.Sira);
        });
        
        modelBuilder.Entity<Emlak>(entity =>
        {
            entity.Property(e => e.Açıklama)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.GenelBilgiler)
                .HasColumnType("nvarchar(max)");
        });
        
        modelBuilder.Entity<Emlak>(entity =>
        {
            entity.Property(e => e.GirisSaati)
                .HasColumnType("time(0)");

            entity.Property(e => e.CikisSaati)
                .HasColumnType("time(0)");

            entity.Property(e => e.HasarDepozitosu)
                .HasPrecision(18, 2);

            entity.Property(e => e.TemizlikUcreti)
                .HasPrecision(18, 2);

            entity.Property(e => e.MinimumKonaklamaSuresi)
                .HasDefaultValue(1);
            entity.Property(e => e.MinimumKonaklamaTemizlik)
                .HasDefaultValue(0);
        });
        
        modelBuilder.Entity<Rezervasyon>(entity =>
        {
            entity.Property(r => r.TemizlikUcreti)
                .HasPrecision(18, 2);

            entity.Property(r => r.HasarDepozitosu)
                .HasPrecision(18, 2);
        });
        
        modelBuilder.Entity<AnasayfaBolum>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Baslik)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Slug)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(x => x.Slug)
                .IsUnique();

            entity.Property(x => x.Aciklama)
                .HasMaxLength(500);

            entity.Property(x => x.GosterimTipi)
                .HasConversion<int>();

            entity.Property(x => x.SecimTipi)
                .HasConversion<int>();

            entity.Property(x => x.Aktif)
                .HasDefaultValue(true);

            entity.Property(x => x.MaksimumVillaSayisi)
                .HasDefaultValue(10);
        });

        modelBuilder.Entity<AnasayfaBolumEmlak>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new
            {
                x.AnasayfaBolumId,
                x.EmlakId
            }).IsUnique();

            entity.HasOne(x => x.AnasayfaBolum)
                .WithMany(x => x.Emlaklar)
                .HasForeignKey(x => x.AnasayfaBolumId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Emlak>()
                .WithMany()
                .HasForeignKey(x => x.EmlakId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Emlak>(entity =>
        {
            entity.Property(e => e.Yayinda)
                .HasDefaultValue(false);

            entity.Property(e => e.OtoparkSayisi)
                .HasDefaultValue(0);

            entity.Property(e => e.BelgeNo)
                .HasMaxLength(100);
        });
        
        modelBuilder.Entity<EmlakOzellik>()
            .Property(x => x.LogoUrl)
            .HasMaxLength(500);

    }
}