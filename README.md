# Gold Fitness Spor Salonu Yönetim Sistemi

## Proje Hakkında
Bu proje, Sakarya Üniversitesi Web Programlama dersi kapsamında geliştirilmiş bir Spor Salonu (Fitness Center) Yönetim ve Randevu Sistemidir.

## Özellikler

### 1. Spor Salonu Yönetimi
- Salon bilgileri (ad, adres, çalışma saatleri)
- Hizmet tanımlamaları (fitness, yoga, pilates vb.)
- Hizmet ücretleri ve süreleri

### 2. Antrenör Yönetimi
- Antrenör bilgileri ve uzmanlık alanları
- Mesai saatleri tanımlama
- Antrenör-Hizmet eşleştirme

### 3. Üye ve Randevu Sistemi
- Üye kayıt ve giriş
- Online randevu alma
- Randevu çakışma kontrolü
- Randevu onay mekanizması

### 4. REST API
- LINQ sorguları ile veri filtreleme
- Antrenör listeleme API
- Müsait antrenör sorgulama
- Randevu filtreleme API

### 5. Yapay Zeka Entegrasyonu
- Kişiselleştirilmiş egzersiz önerileri
- Diyet planı önerileri
- BMI hesaplama

## Kullanılan Teknolojiler

- **Backend:** ASP.NET Core 8.0 MVC
- **ORM:** Entity Framework Core 8.0
- **Veritabanı:** SQL Server (LocalDB)
- **Kimlik Doğrulama:** ASP.NET Core Identity
- **Frontend:** Bootstrap 5, HTML5, CSS3, JavaScript, jQuery
- **API:** REST API
- **Yapay Zeka:** Google Gemini API

## Kurulum

### Gereksinimler
- .NET 8.0 SDK
- SQL Server (LocalDB)
- Visual Studio 2022 veya VS Code

### Adımlar

1. Projeyi klonlayın:
```bash
git clone https://github.com/kullaniciadi/GoldFitness.git
cd SporSalonu
```

2. NuGet paketlerini yükleyin:
```bash
dotnet restore
```

3. Veritabanını oluşturun:
```bash
dotnet ef database update
```

4. Projeyi çalıştırın:
```bash
dotnet run
```

5. Tarayıcıda açın: `https://localhost:5001`

## Varsayılan Kullanıcılar

### Admin
- **E-posta:** b231210068@sakarya.edu.tr
- **Şifre:** sau123

## Proje Yapısı

```
SporSalonu/
├── Controllers/
│   ├── HomeController.cs
│   ├── HesapController.cs
│   ├── RandevuController.cs
│   ├── YonetimController.cs
│   ├── YapayZekaController.cs
│   └── ApiController.cs
├── Models/
│   ├── Salon.cs
│   ├── Hizmet.cs
│   ├── Antrenor.cs
│   ├── Randevu.cs
│   ├── Uye.cs
│   └── ViewModels/
├── Views/
│   ├── Home/
│   ├── Hesap/
│   ├── Randevu/
│   ├── Yonetim/
│   └── YapayZeka/
├── Data/
│   └── UygulamaDbContext.cs
└── wwwroot/
```

## API Endpoint'leri

| Endpoint | Açıklama |
|----------|----------|
| GET /api/Api/antrenorler | Tüm antrenörleri listeler |
| GET /api/Api/musait-antrenorler?tarih=2025-01-15 | Müsait antrenörleri getirir |
| GET /api/Api/hizmetler | Tüm hizmetleri listeler |
| GET /api/Api/uye-randevulari/{uyeId} | Üyenin randevularını getirir |
| GET /api/Api/randevular?baslangic=...&bitis=... | Randevuları filtreler |
| GET /api/Api/istatistikler | İstatistikleri getirir |

## Veritabanı Modeli

- **Salonlar** → Spor salonu bilgileri
- **Hizmetler** → Sunulan hizmetler (1-N: Salon)
- **Antrenorler** → Eğitmen bilgileri (1-N: Salon)
- **AntrenorHizmetleri** → Antrenör-Hizmet ilişkisi (N-N)
- **AspNetUsers** → Üye bilgileri (Identity)
- **Randevular** → Randevu kayıtları

## Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## Geliştirici

- **Öğrenci No:** B231210068
- **Ders:** Web Programlama
- **Dönem:** 2024-2025 Güz
