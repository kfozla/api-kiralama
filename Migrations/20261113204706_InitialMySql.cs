using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_kiralama.Migrations
{
    /// <inheritdoc />
    public partial class InitialMySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AnasayfaBolumler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Baslik = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Aciklama = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GosterimTipi = table.Column<int>(type: "int", nullable: false),
                    SecimTipi = table.Column<int>(type: "int", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    MaksimumVillaSayisi = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    Aktif = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnasayfaBolumler", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BankaHesaplari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BankaAdi = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HesapSahibi = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Iban = table.Column<string>(type: "varchar(34)", maxLength: 34, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubeAdi = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParaBirimi = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LogoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    AktifMi = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankaHesaplari", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Bolgeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Ad = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentBolgeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bolgeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bolgeler_Bolgeler_ParentBolgeId",
                        column: x => x.ParentBolgeId,
                        principalTable: "Bolgeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmlakOzellikleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OzellikAdi = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Kategori = table.Column<int>(type: "int", nullable: false),
                    LogoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakOzellikleri", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmlakSahipleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Ad = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Soyad = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefon = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Aciklama = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KayitTarihi = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakSahipleri", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IsletmeAyarlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsletmeAdi = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    YetkiliKisi = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefon = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Adres = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WebSitesi = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LogoUrl = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TursabUyeligiVar = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TursabBelgeNo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TursabBelgeUrl = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsletmeAyarlari", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Expires = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsRevoked = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SayfaIcerikleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Baslik = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IcerikHtml = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SayfaIcerikleri", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SikcaSorulanSorular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Soru = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CevapHtml = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    AktifMi = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SikcaSorulanSorular", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Emlaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmlakSahibiId = table.Column<int>(type: "int", nullable: true),
                    EmlakTipi = table.Column<int>(type: "int", nullable: false),
                    BolgeId = table.Column<int>(type: "int", nullable: false),
                    EmlakAdı = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Açıklama = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Kapasite = table.Column<int>(type: "int", nullable: false),
                    YatakOdasıSayısı = table.Column<int>(type: "int", nullable: false),
                    BanyoSayısı = table.Column<int>(type: "int", nullable: false),
                    GenelBilgiler = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<double>(type: "double", nullable: true),
                    Longitude = table.Column<double>(type: "double", nullable: true),
                    GirisSaati = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    CikisSaati = table.Column<TimeOnly>(type: "time(0)", nullable: false),
                    HasarDepozitosu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TemizlikUcreti = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinimumKonaklamaSuresi = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MinimumKonaklamaTemizlik = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Yayinda = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    OtoparkSayisi = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BelgeNo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emlaklar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emlaklar_Bolgeler_BolgeId",
                        column: x => x.BolgeId,
                        principalTable: "Bolgeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Emlaklar_EmlakSahipleri_EmlakSahibiId",
                        column: x => x.EmlakSahibiId,
                        principalTable: "EmlakSahipleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AnasayfaBolumEmlaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AnasayfaBolumId = table.Column<int>(type: "int", nullable: false),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnasayfaBolumEmlaklar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnasayfaBolumEmlaklar_AnasayfaBolumler_AnasayfaBolumId",
                        column: x => x.AnasayfaBolumId,
                        principalTable: "AnasayfaBolumler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnasayfaBolumEmlaklar_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmlakFiyatlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    BaslangicTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    BitisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    GecelikFiyat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakFiyatlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmlakFiyatlari_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmlakFotograflari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    DosyaYolu = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    KapakMi = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    KapakEmlakId = table.Column<int>(type: "int", nullable: true, computedColumnSql: "CASE WHEN `KapakMi` = 1 THEN `EmlakId` ELSE NULL END", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakFotograflari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmlakFotograflari_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmlakHavuzBilgileri",
                columns: table => new
                {
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    HavuzTipi = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    En = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Boy = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Derinlik = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    HavuzKorunakTipi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakHavuzBilgileri", x => new { x.EmlakId, x.Id });
                    table.ForeignKey(
                        name: "FK_EmlakHavuzBilgileri_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmlakOneCikanOzellikIliskileri",
                columns: table => new
                {
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    OneCıkanOzelliklerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakOneCikanOzellikIliskileri", x => new { x.EmlakId, x.OneCıkanOzelliklerId });
                    table.ForeignKey(
                        name: "FK_EmlakOneCikanOzellikIliskileri_EmlakOzellikleri_OneCıkanOzel~",
                        column: x => x.OneCıkanOzelliklerId,
                        principalTable: "EmlakOzellikleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmlakOneCikanOzellikIliskileri_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmlakOzellikIliskileri",
                columns: table => new
                {
                    EmlakOzellikleriId = table.Column<int>(type: "int", nullable: false),
                    EmlaklarId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakOzellikIliskileri", x => new { x.EmlakOzellikleriId, x.EmlaklarId });
                    table.ForeignKey(
                        name: "FK_EmlakOzellikIliskileri_EmlakOzellikleri_EmlakOzellikleriId",
                        column: x => x.EmlakOzellikleriId,
                        principalTable: "EmlakOzellikleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmlakOzellikIliskileri_Emlaklar_EmlaklarId",
                        column: x => x.EmlaklarId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rezervasyonlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    MusteriAd = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MusteriSoyad = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefon = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GirisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    CikisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    KisiSayisi = table.Column<int>(type: "int", nullable: false),
                    ToplamFiyat = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TemizlikUcreti = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HasarDepozitosu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rezervasyonlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rezervasyonlar_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmlakBlokeTarihler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmlakId = table.Column<int>(type: "int", nullable: false),
                    RezervasyonId = table.Column<int>(type: "int", nullable: true),
                    BaslangicTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    BitisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    Aciklama = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmlakBlokeTarihler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmlakBlokeTarihler_Emlaklar_EmlakId",
                        column: x => x.EmlakId,
                        principalTable: "Emlaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmlakBlokeTarihler_Rezervasyonlar_RezervasyonId",
                        column: x => x.RezervasyonId,
                        principalTable: "Rezervasyonlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AnasayfaBolumEmlaklar_AnasayfaBolumId_EmlakId",
                table: "AnasayfaBolumEmlaklar",
                columns: new[] { "AnasayfaBolumId", "EmlakId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnasayfaBolumEmlaklar_EmlakId",
                table: "AnasayfaBolumEmlaklar",
                column: "EmlakId");

            migrationBuilder.CreateIndex(
                name: "IX_AnasayfaBolumler_Slug",
                table: "AnasayfaBolumler",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankaHesaplari_Sira",
                table: "BankaHesaplari",
                column: "Sira");

            migrationBuilder.CreateIndex(
                name: "IX_Bolgeler_ParentBolgeId",
                table: "Bolgeler",
                column: "ParentBolgeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakBlokeTarihler_EmlakId",
                table: "EmlakBlokeTarihler",
                column: "EmlakId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakBlokeTarihler_RezervasyonId",
                table: "EmlakBlokeTarihler",
                column: "RezervasyonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmlakFiyatlari_EmlakId",
                table: "EmlakFiyatlari",
                column: "EmlakId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakFotograflari_EmlakId",
                table: "EmlakFotograflari",
                column: "EmlakId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakFotograflari_EmlakId_Sira",
                table: "EmlakFotograflari",
                columns: new[] { "EmlakId", "Sira" });

            migrationBuilder.CreateIndex(
                name: "IX_EmlakFotograflari_KapakEmlakId",
                table: "EmlakFotograflari",
                column: "KapakEmlakId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Emlaklar_BolgeId",
                table: "Emlaklar",
                column: "BolgeId");

            migrationBuilder.CreateIndex(
                name: "IX_Emlaklar_EmlakSahibiId",
                table: "Emlaklar",
                column: "EmlakSahibiId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakOneCikanOzellikIliskileri_OneCıkanOzelliklerId",
                table: "EmlakOneCikanOzellikIliskileri",
                column: "OneCıkanOzelliklerId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakOzellikIliskileri_EmlaklarId",
                table: "EmlakOzellikIliskileri",
                column: "EmlaklarId");

            migrationBuilder.CreateIndex(
                name: "IX_EmlakOzellikleri_OzellikAdi_Kategori",
                table: "EmlakOzellikleri",
                columns: new[] { "OzellikAdi", "Kategori" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_CreatedAt_Id",
                table: "Rezervasyonlar",
                columns: new[] { "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_Durum_GirisTarihi",
                table: "Rezervasyonlar",
                columns: new[] { "Durum", "GirisTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervasyonlar_EmlakId",
                table: "Rezervasyonlar",
                column: "EmlakId");

            migrationBuilder.CreateIndex(
                name: "IX_SayfaIcerikleri_Slug",
                table: "SayfaIcerikleri",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SikcaSorulanSorular_Sira",
                table: "SikcaSorulanSorular",
                column: "Sira");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnasayfaBolumEmlaklar");

            migrationBuilder.DropTable(
                name: "BankaHesaplari");

            migrationBuilder.DropTable(
                name: "EmlakBlokeTarihler");

            migrationBuilder.DropTable(
                name: "EmlakFiyatlari");

            migrationBuilder.DropTable(
                name: "EmlakFotograflari");

            migrationBuilder.DropTable(
                name: "EmlakHavuzBilgileri");

            migrationBuilder.DropTable(
                name: "EmlakOneCikanOzellikIliskileri");

            migrationBuilder.DropTable(
                name: "EmlakOzellikIliskileri");

            migrationBuilder.DropTable(
                name: "IsletmeAyarlari");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "SayfaIcerikleri");

            migrationBuilder.DropTable(
                name: "SikcaSorulanSorular");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AnasayfaBolumler");

            migrationBuilder.DropTable(
                name: "Rezervasyonlar");

            migrationBuilder.DropTable(
                name: "EmlakOzellikleri");

            migrationBuilder.DropTable(
                name: "Emlaklar");

            migrationBuilder.DropTable(
                name: "Bolgeler");

            migrationBuilder.DropTable(
                name: "EmlakSahipleri");
        }
    }
}
