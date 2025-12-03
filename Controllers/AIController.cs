using FitnessCenter.Data;
using FitnessCenter.Models;
using FitnessCenter.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FitnessCenter.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly HttpClient _httpClient;

        public AIController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _environment = environment;
            _httpClient = new HttpClient();
        }

        // GET: /AI/Recommendation
        public async Task<IActionResult> Recommendation()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new AIRecommendationViewModel
            {
                Height = user?.Height,
                Weight = user?.Weight != null ? (int?)user.Weight : null
            };
            return View(model);
        }

        // POST: /AI/GetRecommendation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRecommendation(AIRecommendationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Recommendation", model);
            }

            var user = await _userManager.GetUserAsync(User);
            string? uploadedImagePath = null;

            // Fotoğraf yüklenmiş mi?
            if (model.Image != null && model.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "ai");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{model.Image.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(fileStream);
                }

                uploadedImagePath = $"/uploads/ai/{uniqueFileName}";
            }

            // BMI hesapla
            double? bmi = null;
            string? bmiCategory = null;
            if (model.Height.HasValue && model.Weight.HasValue)
            {
                var heightInMeters = model.Height.Value / 100.0;
                bmi = model.Weight.Value / (heightInMeters * heightInMeters);
                bmiCategory = GetBMICategory(bmi.Value);
            }

            // AI önerisi oluştur (OpenAI API veya yerel öneri sistemi)
            var recommendation = await GenerateRecommendation(model, bmi, bmiCategory);

            // Veritabanına kaydet
            var aiRecommendation = new AIExerciseRecommendation
            {
                UserId = user!.Id,
                BodyType = model.BodyType,
                Height = model.Height,
                Weight = model.Weight,
                Goal = model.Goal,
                UploadedImageUrl = uploadedImagePath,
                Recommendation = recommendation
            };

            _context.AIExerciseRecommendations.Add(aiRecommendation);
            await _context.SaveChangesAsync();

            var result = new AIRecommendationResultViewModel
            {
                Recommendation = recommendation,
                UploadedImageUrl = uploadedImagePath,
                BodyType = model.BodyType,
                Goal = model.Goal,
                Height = model.Height,
                Weight = model.Weight,
                BMI = bmi,
                BMICategory = bmiCategory
            };

            return View("Result", result);
        }

        private async Task<string> GenerateRecommendation(AIRecommendationViewModel model, double? bmi, string? bmiCategory)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            // OpenAI API anahtarı varsa kullan
            if (!string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_OPENAI_API_KEY_HERE")
            {
                try
                {
                    return await GetOpenAIRecommendation(model, bmi, bmiCategory, apiKey);
                }
                catch (Exception ex)
                {
                    // API hatası durumunda yerel öneri sistemine geç
                    Console.WriteLine($"OpenAI API hatası: {ex.Message}");
                }
            }

            // Yerel öneri sistemi
            return GenerateLocalRecommendation(model, bmi, bmiCategory);
        }

        private async Task<string> GetOpenAIRecommendation(AIRecommendationViewModel model, double? bmi, string? bmiCategory, string apiKey)
        {
            var prompt = BuildPrompt(model, bmi, bmiCategory);

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "Sen profesyonel bir fitness ve sağlık danışmanısın. Türkçe yanıt ver." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 1000,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseString);
                var messageContent = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return messageContent ?? GenerateLocalRecommendation(model, bmi, bmiCategory);
            }

            throw new Exception($"OpenAI API hatası: {responseString}");
        }

        private string BuildPrompt(AIRecommendationViewModel model, double? bmi, string? bmiCategory)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Aşağıdaki bilgilere göre kişiselleştirilmiş bir egzersiz ve beslenme programı öner:");
            sb.AppendLine();

            if (model.Height.HasValue)
                sb.AppendLine($"- Boy: {model.Height} cm");
            if (model.Weight.HasValue)
                sb.AppendLine($"- Kilo: {model.Weight} kg");
            if (bmi.HasValue)
                sb.AppendLine($"- Vücut Kitle İndeksi (BMI): {bmi:F1} ({bmiCategory})");
            if (model.BodyType.HasValue)
                sb.AppendLine($"- Vücut Tipi: {GetBodyTypeDescription(model.BodyType.Value)}");
            if (model.Goal.HasValue)
                sb.AppendLine($"- Hedef: {GetGoalDescription(model.Goal.Value)}");
            if (!string.IsNullOrEmpty(model.AdditionalInfo))
                sb.AppendLine($"- Ek Bilgiler: {model.AdditionalInfo}");

            sb.AppendLine();
            sb.AppendLine("Lütfen şunları içeren detaylı bir program hazırla:");
            sb.AppendLine("1. Haftalık egzersiz programı (hangi günler hangi egzersizler)");
            sb.AppendLine("2. Beslenme önerileri");
            sb.AppendLine("3. Dikkat edilmesi gerekenler");
            sb.AppendLine("4. Tahmini sonuç süresi");

            return sb.ToString();
        }

        private string GenerateLocalRecommendation(AIRecommendationViewModel model, double? bmi, string? bmiCategory)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## 🏋️ Kişiselleştirilmiş Fitness Programınız\n");

            // BMI Bilgisi
            if (bmi.HasValue)
            {
                sb.AppendLine($"### 📊 Vücut Analizi");
                sb.AppendLine($"- **BMI (Vücut Kitle İndeksi):** {bmi:F1}");
                sb.AppendLine($"- **Kategori:** {bmiCategory}");
                sb.AppendLine();
            }

            // Hedef bazlı öneriler
            sb.AppendLine("### 🎯 Egzersiz Programı\n");

            switch (model.Goal)
            {
                case FitnessGoal.WeightLoss:
                    sb.AppendLine("**Kilo Verme Programı:**\n");
                    sb.AppendLine("**Pazartesi - Kardio:** 45 dk koşu bandı veya eliptik + 15 dk HIIT");
                    sb.AppendLine("**Salı - Üst Vücut:** Göğüs, sırt, omuz egzersizleri (3x15 tekrar)");
                    sb.AppendLine("**Çarşamba - Kardio:** 30 dk yüzme veya bisiklet + 20 dk core");
                    sb.AppendLine("**Perşembe - Alt Vücut:** Bacak, kalça egzersizleri (3x15 tekrar)");
                    sb.AppendLine("**Cuma - HIIT:** 30 dk yüksek yoğunluklu interval antrenman");
                    sb.AppendLine("**Cumartesi - Aktif Dinlenme:** Yürüyüş, yoga veya esneme");
                    sb.AppendLine("**Pazar - Dinlenme**");
                    break;

                case FitnessGoal.MuscleGain:
                    sb.AppendLine("**Kas Geliştirme Programı:**\n");
                    sb.AppendLine("**Pazartesi - Göğüs & Triceps:** Bench press, dumbbell fly, triceps pushdown (4x8-10)");
                    sb.AppendLine("**Salı - Sırt & Biceps:** Lat pulldown, barbell row, biceps curl (4x8-10)");
                    sb.AppendLine("**Çarşamba - Bacak:** Squat, leg press, lunges (4x8-10)");
                    sb.AppendLine("**Perşembe - Omuz & Trapez:** Shoulder press, lateral raise (4x8-10)");
                    sb.AppendLine("**Cuma - Kol:** Biceps, triceps ve önkol çalışması");
                    sb.AppendLine("**Cumartesi - Tam Vücut:** Compound hareketler");
                    sb.AppendLine("**Pazar - Dinlenme**");
                    break;

                case FitnessGoal.Endurance:
                    sb.AppendLine("**Kondisyon Artırma Programı:**\n");
                    sb.AppendLine("**Pazartesi:** 40 dk tempolu koşu");
                    sb.AppendLine("**Salı:** 45 dk yüzme");
                    sb.AppendLine("**Çarşamba:** 50 dk bisiklet");
                    sb.AppendLine("**Perşembe:** İnterval koşu (8x400m)");
                    sb.AppendLine("**Cuma:** 60 dk düşük tempolu uzun koşu");
                    sb.AppendLine("**Cumartesi:** Cross training (karışık spor)");
                    sb.AppendLine("**Pazar - Dinlenme**");
                    break;

                case FitnessGoal.Flexibility:
                    sb.AppendLine("**Esneklik Programı:**\n");
                    sb.AppendLine("**Her gün:** 15 dk sabah esnetme rutini");
                    sb.AppendLine("**Pazartesi, Çarşamba, Cuma:** 60 dk yoga dersi");
                    sb.AppendLine("**Salı, Perşembe:** 45 dk pilates");
                    sb.AppendLine("**Cumartesi:** Foam roller ile recovery");
                    sb.AppendLine("**Pazar:** Hafif yürüyüş ve meditasyon");
                    break;

                default:
                    sb.AppendLine("**Genel Sağlık Programı:**\n");
                    sb.AppendLine("**Pazartesi:** 30 dk kardio + 20 dk kuvvet");
                    sb.AppendLine("**Salı:** 45 dk yürüyüş");
                    sb.AppendLine("**Çarşamba:** 30 dk tam vücut antrenman");
                    sb.AppendLine("**Perşembe:** 40 dk yüzme veya bisiklet");
                    sb.AppendLine("**Cuma:** 30 dk HIIT");
                    sb.AppendLine("**Cumartesi:** Aktif dinlenme");
                    sb.AppendLine("**Pazar:** Yoga veya esneme");
                    break;
            }

            // Beslenme önerileri
            sb.AppendLine("\n### 🥗 Beslenme Önerileri\n");
            if (model.Goal == FitnessGoal.WeightLoss)
            {
                sb.AppendLine("- Günlük kalori açığı: 300-500 kcal");
                sb.AppendLine("- Protein: Vücut ağırlığının kg başına 1.6-2g");
                sb.AppendLine("- Bol sebze ve meyve tüketin");
                sb.AppendLine("- İşlenmiş gıdalardan kaçının");
                sb.AppendLine("- Günde en az 2.5-3 litre su için");
            }
            else if (model.Goal == FitnessGoal.MuscleGain)
            {
                sb.AppendLine("- Günlük kalori fazlası: 300-500 kcal");
                sb.AppendLine("- Protein: Vücut ağırlığının kg başına 2-2.2g");
                sb.AppendLine("- Kompleks karbonhidratları tercih edin");
                sb.AppendLine("- Sağlıklı yağlar (zeytinyağı, avokado, balık)");
                sb.AppendLine("- Antrenman sonrası protein alımına dikkat");
            }
            else
            {
                sb.AppendLine("- Dengeli ve düzenli beslenin");
                sb.AppendLine("- Her öğünde protein kaynağı bulundurun");
                sb.AppendLine("- Bol su tüketin (günde 2-3 litre)");
                sb.AppendLine("- Sebze ve meyve ağırlıklı beslenin");
                sb.AppendLine("- Şeker ve işlenmiş gıdaları sınırlayın");
            }

            // Uyarılar
            sb.AppendLine("\n### ⚠️ Dikkat Edilecekler\n");
            sb.AppendLine("- Programa başlamadan önce doktorunuza danışın");
            sb.AppendLine("- Ağrı hissettiğinizde durun");
            sb.AppendLine("- Yeterli uyku alın (7-8 saat)");
            sb.AppendLine("- Düzenli olun, tutarlılık başarının anahtarıdır");
            sb.AppendLine("- İlerlemenizi takip edin");

            return sb.ToString();
        }

        private string GetBMICategory(double bmi)
        {
            return bmi switch
            {
                < 18.5 => "Zayıf",
                < 25 => "Normal",
                < 30 => "Fazla Kilolu",
                < 35 => "Obez (Sınıf 1)",
                < 40 => "Obez (Sınıf 2)",
                _ => "Aşırı Obez (Sınıf 3)"
            };
        }

        private string GetBodyTypeDescription(BodyType bodyType)
        {
            return bodyType switch
            {
                BodyType.Ectomorph => "Ektomorf (İnce yapılı, hızlı metabolizma)",
                BodyType.Mesomorph => "Mezomorf (Atletik yapılı, kolay kas yapar)",
                BodyType.Endomorph => "Endomorf (Geniş yapılı, yavaş metabolizma)",
                _ => "Belirtilmemiş"
            };
        }

        private string GetGoalDescription(FitnessGoal goal)
        {
            return goal switch
            {
                FitnessGoal.WeightLoss => "Kilo Verme",
                FitnessGoal.MuscleGain => "Kas Geliştirme",
                FitnessGoal.Endurance => "Kondisyon Artırma",
                FitnessGoal.Flexibility => "Esneklik Kazanma",
                FitnessGoal.GeneralHealth => "Genel Sağlık",
                _ => "Belirtilmemiş"
            };
        }
    }
}

