CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `AnasayfaBolumler` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Baslik` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `Aciklama` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Slug` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `GosterimTipi` int NOT NULL,
    `SecimTipi` int NOT NULL,
    `Sira` int NOT NULL,
    `MaksimumVillaSayisi` int NOT NULL DEFAULT 10,
    `Aktif` tinyint(1) NOT NULL DEFAULT TRUE,
    CONSTRAINT `PK_AnasayfaBolumler` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `BankaHesaplari` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `BankaAdi` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `HesapSahibi` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Iban` varchar(34) CHARACTER SET utf8mb4 NOT NULL,
    `SubeAdi` varchar(150) CHARACTER SET utf8mb4 NULL,
    `ParaBirimi` varchar(3) CHARACTER SET utf8mb4 NOT NULL,
    `LogoUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Sira` int NOT NULL,
    `AktifMi` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_BankaHesaplari` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Bolgeler` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Ad` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ParentBolgeId` int NULL,
    CONSTRAINT `PK_Bolgeler` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Bolgeler_Bolgeler_ParentBolgeId` FOREIGN KEY (`ParentBolgeId`) REFERENCES `Bolgeler` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `EmlakOzellikleri` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OzellikAdi` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `Kategori` int NOT NULL,
    `LogoUrl` varchar(500) CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_EmlakOzellikleri` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `EmlakSahipleri` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Ad` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Soyad` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Telefon` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `Aciklama` longtext CHARACTER SET utf8mb4 NULL,
    `KayitTarihi` datetime(6) NOT NULL,
    CONSTRAINT `PK_EmlakSahipleri` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `IsletmeAyarlari` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `IsletmeAdi` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `YetkiliKisi` varchar(150) CHARACTER SET utf8mb4 NULL,
    `Telefon` longtext CHARACTER SET utf8mb4 NULL,
    `Email` varchar(200) CHARACTER SET utf8mb4 NULL,
    `Adres` longtext CHARACTER SET utf8mb4 NULL,
    `WebSitesi` longtext CHARACTER SET utf8mb4 NULL,
    `LogoUrl` varchar(255) CHARACTER SET utf8mb4 NULL,
    `TursabUyeligiVar` tinyint(1) NOT NULL,
    `TursabBelgeNo` varchar(100) CHARACTER SET utf8mb4 NULL,
    `TursabBelgeUrl` varchar(255) CHARACTER SET utf8mb4 NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_IsletmeAyarlari` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `RefreshTokens` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `Token` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Expires` datetime(6) NOT NULL,
    `Created` datetime(6) NOT NULL,
    `IsRevoked` tinyint(1) NOT NULL,
    CONSTRAINT `PK_RefreshTokens` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `SayfaIcerikleri` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Slug` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Baslik` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `IcerikHtml` longtext CHARACTER SET utf8mb4 NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_SayfaIcerikleri` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `SikcaSorulanSorular` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Soru` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `CevapHtml` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Sira` int NOT NULL,
    `AktifMi` tinyint(1) NOT NULL,
    CONSTRAINT `PK_SikcaSorulanSorular` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Users` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Username` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Password` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
    `Role` longtext CHARACTER SET utf8mb4 NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Emlaklar` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EmlakSahibiId` int NULL,
    `EmlakTipi` int NOT NULL,
    `BolgeId` int NOT NULL,
    `EmlakAdı` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Açıklama` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Kapasite` int NOT NULL,
    `YatakOdasıSayısı` int NOT NULL,
    `BanyoSayısı` int NOT NULL,
    `GenelBilgiler` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Latitude` double NULL,
    `Longitude` double NULL,
    `GirisSaati` time(0) NOT NULL,
    `CikisSaati` time(0) NOT NULL,
    `HasarDepozitosu` decimal(18,2) NOT NULL,
    `TemizlikUcreti` decimal(18,2) NOT NULL,
    `MinimumKonaklamaSuresi` int NOT NULL DEFAULT 1,
    `MinimumKonaklamaTemizlik` int NOT NULL DEFAULT 0,
    `Yayinda` tinyint(1) NOT NULL DEFAULT FALSE,
    `OtoparkSayisi` int NOT NULL DEFAULT 0,
    `BelgeNo` varchar(100) CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    `DeletedAt` datetime(6) NULL,
    `IsDeleted` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Emlaklar` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Emlaklar_Bolgeler_BolgeId` FOREIGN KEY (`BolgeId`) REFERENCES `Bolgeler` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Emlaklar_EmlakSahipleri_EmlakSahibiId` FOREIGN KEY (`EmlakSahibiId`) REFERENCES `EmlakSahipleri` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `AnasayfaBolumEmlaklar` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `AnasayfaBolumId` int NOT NULL,
    `EmlakId` int NOT NULL,
    `Sira` int NOT NULL,
    CONSTRAINT `PK_AnasayfaBolumEmlaklar` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AnasayfaBolumEmlaklar_AnasayfaBolumler_AnasayfaBolumId` FOREIGN KEY (`AnasayfaBolumId`) REFERENCES `AnasayfaBolumler` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AnasayfaBolumEmlaklar_Emlaklar_EmlakId` FOREIGN KEY (`EmlakId`) REFERENCES `Emlaklar` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `EmlakFiyatlari` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EmlakId` int NOT NULL,
    `BaslangicTarihi` date NOT NULL,
    `BitisTarihi` date NOT NULL,
    `GecelikFiyat` decimal(18,2) NOT NULL,
    CONSTRAINT `PK_EmlakFiyatlari` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_EmlakFiyatlari_Emlaklar_EmlakId` FOREIGN KEY (`EmlakId`) REFERENCES `Emlaklar` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `EmlakFotograflari` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EmlakId` int NOT NULL,
    `DosyaYolu` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `Sira` int NOT NULL,
    `KapakMi` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `KapakEmlakId` int AS (CASE WHEN `KapakMi` = 1 THEN `EmlakId` ELSE NULL END) NULL,
    CONSTRAINT `PK_EmlakFotograflari` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_EmlakFotograflari_Emlaklar_EmlakId` FOREIGN KEY (`EmlakId`) REFERENCES `Emlaklar` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `EmlakHavuzBilgileri` (
    `EmlakId` int NOT NULL,
    `Id` int NOT NULL AUTO_INCREMENT,
    `HavuzTipi` longtext CHARACTER SET utf8mb4 NOT NULL,
    `En` decimal(65,30) NOT NULL,
    `Boy` decimal(65,30) NOT NULL,
    `Derinlik` decimal(65,30) NOT NULL,
    `HavuzKorunakTipi` int NOT NULL,
    CONSTRAINT `PK_EmlakHavuzBilgileri` PRIMARY KEY (`Id`, `EmlakId`),
    CONSTRAINT `FK_EmlakHavuzBilgileri_Emlaklar_EmlakId` FOREIGN KEY (`EmlakId`) REFERENCES `Emlaklar` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `EmlakOneCikanOzellikIliskileri` (
    `EmlakId` int NOT NULL,
    `OneCıkanOzelliklerId` int NOT NULL,
    CONSTRAINT `PK_EmlakOneCikanOzellikIliskileri` PRIMARY KEY (`EmlakId`, `OneCıkanOzelliklerId`),
    CONSTRAINT `FK_EmlakOneCikanOzellikIliskileri_EmlakOzellikleri_OneCıkanOzel~` FOREIGN KEY (`OneCıkanOzelliklerId`) REFERENCES `EmlakOzellikleri` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_EmlakOneCikanOzellikIliskileri_Emlaklar_EmlakId` FOREIGN KEY (`EmlakId`) REFERENCES `Emlaklar` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `EmlakOzellikIliskileri` (
    `EmlakOzellikleriId` int NOT NULL,
    `EmlaklarId` int NOT NULL,
    CONSTRAINT `PK_EmlakOzellikIliskileri` PRIMARY KEY (`EmlakOzellikleriId`, `EmlaklarId`),
    CONSTRAINT `FK_EmlakOzellikIliskileri_EmlakOzellikleri_EmlakOzellikleriId` FOREIGN KEY (`EmlakOzellikleriId`) REFERENCES `EmlakOzellikleri` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_EmlakOzellikIliskileri_Emlaklar_EmlaklarId` FOREIGN KEY (`EmlaklarId`) REFERENCES `Emlaklar` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Rezervasyonlar` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EmlakId` int NOT NULL,
    `MusteriAd` longtext CHARACTER SET utf8mb4 NOT NULL,
    `MusteriSoyad` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Telefon` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `GirisTarihi` date NOT NULL,
    `CikisTarihi` date NOT NULL,
    `KisiSayisi` int NOT NULL,
    `ToplamFiyat` decimal(18,2) NOT NULL,
    `TemizlikUcreti` decimal(18,2) NOT NULL,
    `HasarDepozitosu` decimal(18,2) NOT NULL,
    `Durum` int NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Rezervasyonlar` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Rezervasyonlar_Emlaklar_EmlakId` FOREIGN KEY (`EmlakId`) REFERENCES `Emlaklar` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `EmlakBlokeTarihler` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EmlakId` int NOT NULL,
    `RezervasyonId` int NULL,
    `BaslangicTarihi` date NOT NULL,
    `BitisTarihi` date NOT NULL,
    `Aciklama` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_EmlakBlokeTarihler` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_EmlakBlokeTarihler_Emlaklar_EmlakId` FOREIGN KEY (`EmlakId`) REFERENCES `Emlaklar` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_EmlakBlokeTarihler_Rezervasyonlar_RezervasyonId` FOREIGN KEY (`RezervasyonId`) REFERENCES `Rezervasyonlar` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_AnasayfaBolumEmlaklar_AnasayfaBolumId_EmlakId` ON `AnasayfaBolumEmlaklar` (`AnasayfaBolumId`, `EmlakId`);

CREATE INDEX `IX_AnasayfaBolumEmlaklar_EmlakId` ON `AnasayfaBolumEmlaklar` (`EmlakId`);

CREATE UNIQUE INDEX `IX_AnasayfaBolumler_Slug` ON `AnasayfaBolumler` (`Slug`);

CREATE INDEX `IX_BankaHesaplari_Sira` ON `BankaHesaplari` (`Sira`);

CREATE INDEX `IX_Bolgeler_ParentBolgeId` ON `Bolgeler` (`ParentBolgeId`);

CREATE INDEX `IX_EmlakBlokeTarihler_EmlakId` ON `EmlakBlokeTarihler` (`EmlakId`);

CREATE UNIQUE INDEX `IX_EmlakBlokeTarihler_RezervasyonId` ON `EmlakBlokeTarihler` (`RezervasyonId`);

CREATE INDEX `IX_EmlakFiyatlari_EmlakId` ON `EmlakFiyatlari` (`EmlakId`);

CREATE INDEX `IX_EmlakFotograflari_EmlakId` ON `EmlakFotograflari` (`EmlakId`);

CREATE INDEX `IX_EmlakFotograflari_EmlakId_Sira` ON `EmlakFotograflari` (`EmlakId`, `Sira`);

CREATE UNIQUE INDEX `IX_EmlakFotograflari_KapakEmlakId` ON `EmlakFotograflari` (`KapakEmlakId`);

CREATE INDEX `IX_Emlaklar_BolgeId` ON `Emlaklar` (`BolgeId`);

CREATE INDEX `IX_Emlaklar_EmlakSahibiId` ON `Emlaklar` (`EmlakSahibiId`);

CREATE INDEX `IX_EmlakOneCikanOzellikIliskileri_OneCıkanOzelliklerId` ON `EmlakOneCikanOzellikIliskileri` (`OneCıkanOzelliklerId`);

CREATE INDEX `IX_EmlakOzellikIliskileri_EmlaklarId` ON `EmlakOzellikIliskileri` (`EmlaklarId`);

CREATE UNIQUE INDEX `IX_EmlakOzellikleri_OzellikAdi_Kategori` ON `EmlakOzellikleri` (`OzellikAdi`, `Kategori`);

CREATE INDEX `IX_Rezervasyonlar_CreatedAt_Id` ON `Rezervasyonlar` (`CreatedAt`, `Id`);

CREATE INDEX `IX_Rezervasyonlar_Durum_GirisTarihi` ON `Rezervasyonlar` (`Durum`, `GirisTarihi`);

CREATE INDEX `IX_Rezervasyonlar_EmlakId` ON `Rezervasyonlar` (`EmlakId`);

CREATE UNIQUE INDEX `IX_SayfaIcerikleri_Slug` ON `SayfaIcerikleri` (`Slug`);

CREATE INDEX `IX_SikcaSorulanSorular_Sira` ON `SikcaSorulanSorular` (`Sira`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261113204706_InitialMySql', '8.0.22');

COMMIT;

START TRANSACTION;

CREATE TABLE `EmlakPaylasimlar` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `TokenHash` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
    `Baslik` varchar(150) CHARACTER SET utf8mb4 NULL,
    `GirisTarihi` date NULL,
    `CikisTarihi` date NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `ExpiresAt` datetime(6) NOT NULL,
    `IsRevoked` tinyint(1) NOT NULL,
    CONSTRAINT `PK_EmlakPaylasimlar` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `EmlakPaylasimItems` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `EmlakPaylasimId` int NOT NULL,
    `EmlakId` int NOT NULL,
    `Sira` int NOT NULL,
    CONSTRAINT `PK_EmlakPaylasimItems` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_EmlakPaylasimItems_EmlakPaylasimlar_EmlakPaylasimId` FOREIGN KEY (`EmlakPaylasimId`) REFERENCES `EmlakPaylasimlar` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_EmlakPaylasimItems_Emlaklar_EmlakId` FOREIGN KEY (`EmlakId`) REFERENCES `Emlaklar` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_EmlakPaylasimItems_EmlakId` ON `EmlakPaylasimItems` (`EmlakId`);

CREATE UNIQUE INDEX `IX_EmlakPaylasimItems_EmlakPaylasimId_EmlakId` ON `EmlakPaylasimItems` (`EmlakPaylasimId`, `EmlakId`);

CREATE UNIQUE INDEX `IX_EmlakPaylasimlar_TokenHash` ON `EmlakPaylasimlar` (`TokenHash`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261116114301_AddEmlakPaylasim', '8.0.22');

COMMIT;

