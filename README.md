# FitLife - Spor Salonu Yönetim ve Randevu Sistemi

## 📋 Proje Hakkında

Bu proje, ASP.NET Core MVC kullanılarak geliştirilmiş bir Spor Salonu (Fitness Center) Yönetim ve Randevu Sistemidir. Sistem, spor salonlarının sunduğu hizmetleri, antrenörlerin uzmanlık alanlarını, üyelerin randevularını ve yapay zekâ tabanlı egzersiz önerilerini yönetebilmektedir.

## 🚀 Özellikler

### Kullanıcı Özellikleri
- ✅ Üye kayıt ve giriş sistemi
- ✅ Profil yönetimi (boy, kilo, doğum tarihi)
- ✅ Randevu oluşturma ve yönetimi
- ✅ Antrenör ve hizmet görüntüleme
- ✅ Yapay zeka destekli egzersiz önerileri

### Admin Özellikleri
- ✅ Dashboard ile istatistik görüntüleme
- ✅ Antrenör CRUD işlemleri
- ✅ Hizmet CRUD işlemleri
- ✅ Randevu onay/red/tamamlama
- ✅ Üye yönetimi

### Teknik Özellikler
- ✅ REST API (LINQ sorguları ile)
- ✅ Rol bazlı yetkilendirme (Admin, Member)
- ✅ Randevu çakışma kontrolü
- ✅ Veri doğrulama (Client & Server)
- ✅ Modern ve responsive arayüz (Bootstrap 5)

## 🛠️ Kullanılan Teknolojiler

- **Framework:** ASP.NET Core 8.0 MVC
- **Dil:** C#
- **Veritabanı:** SQL Server (LocalDB)
- **ORM:** Entity Framework Core 8.0
- **Kimlik Doğrulama:** ASP.NET Core Identity
- **Frontend:** Bootstrap 5, Bootstrap Icons, jQuery
- **API:** RESTful Web API

## 📦 Kurulum

### Gereksinimler
- .NET 8.0 SDK
- SQL Server (LocalDB veya Express)
- Visual Studio 2022 veya VS Code

### Adımlar

1. **Projeyi klonlayın:**
```bash
git clone [repo-url]
cd FitnessCenter
```

2. **Paketleri yükleyin:**
```bash
dotnet restore
```

3. **Veritabanı bağlantı dizesini kontrol edin:**
`appsettings.json` dosyasındaki ConnectionString'i kendi ortamınıza göre düzenleyin.

4. **Projeyi çalıştırın:**
```bash
dotnet run
```

5. **Tarayıcıda açın:**
```
https://localhost:5001 veya http://localhost:5000
```

## 👤 Varsayılan Kullanıcılar

### Admin Kullanıcısı
- **E-posta:** g221210058@sakarya.edu.tr
- **Şifre:** sau

### Test Üyesi
Kayıt sayfasından yeni üye oluşturabilirsiniz.

## 📁 Proje Yapısı

```
FitnessCenter/
├── Areas/
│   └── Admin/
│       ├── Controllers/
│       └── Views/
├── Controllers/
│   ├── AccountController.cs
│   ├── AppointmentController.cs
│   ├── AIController.cs
│   ├── TrainerController.cs
│   ├── ServiceController.cs
│   └── Api/
│       ├── TrainersApiController.cs
│       ├── ServicesApiController.cs
│       └── AppointmentsApiController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── ApplicationUser.cs
│   ├── Gym.cs
│   ├── Service.cs
│   ├── Trainer.cs
│   ├── Appointment.cs
│   └── ViewModels/
├── Views/
└── wwwroot/
```

## 🔌 API Endpoints

### Antrenörler
- `GET /api/TrainersApi` - Tüm antrenörleri listele
- `GET /api/TrainersApi/{id}` - Belirli antrenörü getir
- `GET /api/TrainersApi/Available?date=2024-01-15` - Müsait antrenörleri getir
- `GET /api/TrainersApi/BySpecialization?specialization=yoga` - Uzmanlık alanına göre filtrele

### Hizmetler
- `GET /api/ServicesApi` - Tüm hizmetleri listele
- `GET /api/ServicesApi/{id}` - Belirli hizmeti getir
- `GET /api/ServicesApi/Categories` - Kategorileri getir
- `GET /api/ServicesApi/PriceRange?min=100&max=500` - Fiyat aralığına göre filtrele

### Randevular
- `GET /api/AppointmentsApi` - Tüm randevuları listele (Admin)
- `GET /api/AppointmentsApi/User/{userId}` - Kullanıcı randevuları
- `GET /api/AppointmentsApi/Trainer/{trainerId}` - Antrenör randevuları
- `GET /api/AppointmentsApi/Statistics` - İstatistikler (Admin)

## 🤖 Yapay Zeka Entegrasyonu

Sistem, kullanıcıların fiziksel bilgilerini (boy, kilo, vücut tipi, hedef) girerek kişiselleştirilmiş egzersiz ve beslenme programı almasını sağlar.

### Özellikler:
- BMI hesaplama ve analizi
- Vücut tipi bazlı öneriler
- Hedef bazlı egzersiz programları
- Beslenme önerileri
- Fotoğraf yükleme desteği

### OpenAI Entegrasyonu (İsteğe Bağlı):
`appsettings.json` dosyasına OpenAI API anahtarınızı ekleyerek daha gelişmiş öneriler alabilirsiniz:
```json
"OpenAI": {
  "ApiKey": "sk-your-api-key-here"
}
```

## 📊 Veritabanı Şeması

- **ApplicationUser:** Kullanıcı bilgileri (Identity ile genişletilmiş)
- **Gym:** Spor salonu bilgileri
- **Service:** Hizmet bilgileri (Fitness, Yoga, Pilates vb.)
- **Trainer:** Antrenör bilgileri
- **TrainerService:** Antrenör-Hizmet ilişkisi (Many-to-Many)
- **TrainerAvailability:** Antrenör müsaitlik saatleri
- **Appointment:** Randevu bilgileri
- **AIExerciseRecommendation:** AI öneri geçmişi

## 📝 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## 👨‍💻 Geliştirici

- **Öğrenci No:** G221210058
- **Ders:** Web Programlama
- **Dönem:** 2024-2025 Güz

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!



