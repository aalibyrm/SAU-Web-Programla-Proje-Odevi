using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SporSalonu.Models.ViewModels;
using System.Text;
using System.Text.Json;

namespace SporSalonu.Controllers
{
    [Authorize]
    public class YapayZekaController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _yapilandirma;
        private readonly ILogger<YapayZekaController> _gunluk;
        private readonly IWebHostEnvironment _ortam;

        public YapayZekaController(
            IHttpClientFactory httpClientFactory,
            IConfiguration yapilandirma,
            ILogger<YapayZekaController> gunluk,
            IWebHostEnvironment ortam)
        {
            _httpClientFactory = httpClientFactory;
            _yapilandirma = yapilandirma;
            _gunluk = gunluk;
            _ortam = ortam;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EgzersizOnerisi()
        {
            return View(new EgzersizOneriViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EgzersizOnerisi(EgzersizOneriViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var promptMetni = EgzersizPromptOlustur(model);
                var cevap = await GeminiMetinSorguGonder(promptMetni);
                model.OneriSonucu = cevap;
            }
            catch (Exception ex)
            {
                _gunluk.LogError(ex, "Yapay zeka sorgusunda hata oluştu");
                model.OneriSonucu = "⚠️ Yapay zeka servisine bağlanırken bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult GorselDonusum()
        {
            return View(new GorselDonusumViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GorselDonusum(GorselDonusumViewModel model)
        {
            if (model.Fotograf == null || model.Fotograf.Length == 0)
            {
                model.HataMesaji = "Lütfen bir fotoğraf yükleyin.";
                return View(model);
            }

            if (model.Fotograf.Length > 5 * 1024 * 1024)
            {
                model.HataMesaji = "Fotoğraf boyutu 5MB'dan küçük olmalıdır.";
                return View(model);
            }

            var izinliTurler = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!izinliTurler.Contains(model.Fotograf.ContentType.ToLower()))
            {
                model.HataMesaji = "Sadece JPG, PNG ve WebP formatları desteklenmektedir.";
                return View(model);
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await model.Fotograf.CopyToAsync(memoryStream);
                var fotografBytes = memoryStream.ToArray();
                var base64Fotograf = Convert.ToBase64String(fotografBytes);
                model.YuklenenFotograf = $"data:{model.Fotograf.ContentType};base64,{base64Fotograf}";

                var sonuc = await QwenGorselDonusumYap(base64Fotograf, model.Hedef);
                
                model.DonusturulmusFotograf = sonuc.GorselBase64;
                model.Aciklama = sonuc.Aciklama;

                if (string.IsNullOrEmpty(model.DonusturulmusFotograf))
                {
                    var geminiAnaliz = await GeminiGorselAnaliz(base64Fotograf, model.Fotograf.ContentType, model.Hedef);
                    model.Aciklama = geminiAnaliz;
                }
            }
            catch (Exception ex)
            {
                _gunluk.LogError(ex, "Görsel dönüşümde hata oluştu");
                model.HataMesaji = "Görsel işlenirken bir hata oluştu. Lütfen tekrar deneyiniz.";
            }

            return View(model);
        }

        private async Task<(string? GorselBase64, string Aciklama)> QwenGorselDonusumYap(string base64Fotograf, HedefTipi hedef)
        {
            var apiAnahtari = _yapilandirma["YapayZekaAyarlari:DashScopeApiKey"];

            if (string.IsNullOrEmpty(apiAnahtari))
            {
                _gunluk.LogWarning("DashScope API anahtarı yapılandırılmamış");
                return (null, "DashScope API anahtarı yapılandırılmamış.");
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(120);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiAnahtari}");

            var hedefPrompt = hedef switch
            {
                HedefTipi.KiloVermek => "Transform this person to look slimmer, leaner, with reduced body fat. Make them look fit and athletic with a toned physique. Keep the same face, clothing style and background.",
                HedefTipi.KasYapmak => "Transform this person to look more muscular, with bigger muscles and a stronger, more athletic physique. Add visible muscle definition to arms, chest and shoulders. Keep the same face, clothing style and background.",
                HedefTipi.FormdaKalmak => "Transform this person to look healthier and more fit, with good posture and a balanced athletic physique. Make them look energetic and healthy. Keep the same face, clothing style and background.",
                _ => "Make this person look healthier and more fit."
            };

            var apiUrl = "https://dashscope-intl.aliyuncs.com/api/v1/services/aigc/multimodal-generation/generation";

            var istek = new
            {
                model = "qwen-image-edit",
                input = new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new { image = $"data:image/jpeg;base64,{base64Fotograf}" },
                                new { text = hedefPrompt }
                            }
                        }
                    }
                },
                parameters = new
                {
                    watermark = false,
                    result_format = "message"
                }
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            var json = JsonSerializer.Serialize(istek);
            var icerik = new StringContent(json, Encoding.UTF8, "application/json");

            _gunluk.LogInformation("DashScope API isteği gönderiliyor...");

            var yanit = await httpClient.PostAsync(apiUrl, icerik);
            var yanitIcerik = await yanit.Content.ReadAsStringAsync();

            _gunluk.LogInformation($"DashScope API yanıtı: {yanit.StatusCode}");

            if (yanit.IsSuccessStatusCode)
            {
                try
                {
                    using var doc = JsonDocument.Parse(yanitIcerik);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("output", out var output))
                    {
                        if (output.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                        {
                            var message = choices[0].GetProperty("message");
                            var content = message.GetProperty("content");

                            string? gorselData = null;
                            string aciklama = "Görsel dönüşümünüz hazır!";

                            foreach (var item in content.EnumerateArray())
                            {
                                if (item.TryGetProperty("image", out var imageUrl))
                                {
                                    var imageUrlStr = imageUrl.GetString();
                                    if (!string.IsNullOrEmpty(imageUrlStr))
                                    {
                                        if (imageUrlStr.StartsWith("data:"))
                                        {
                                            gorselData = imageUrlStr;
                                        }
                                        else
                                        {
                                            var gorselBytes = await httpClient.GetByteArrayAsync(imageUrlStr);
                                            var gorselBase64 = Convert.ToBase64String(gorselBytes);
                                            gorselData = $"data:image/png;base64,{gorselBase64}";
                                        }
                                    }
                                }
                                else if (item.TryGetProperty("text", out var textVal))
                                {
                                    aciklama = textVal.GetString() ?? aciklama;
                                }
                            }

                            var hedefAciklama = hedef switch
                            {
                                HedefTipi.KiloVermek => "🎯 **Kilo Verme Hedefi:** Bu görsel, düzenli egzersiz ve sağlıklı beslenme ile ulaşabileceğiniz fit görünümü göstermektedir.",
                                HedefTipi.KasYapmak => "💪 **Kas Yapma Hedefi:** Bu görsel, güç antrenmanları ve protein açısından zengin beslenme ile ulaşabileceğiniz kaslı görünümü göstermektedir.",
                                HedefTipi.FormdaKalmak => "✨ **Formda Kalma Hedefi:** Bu görsel, aktif yaşam tarzı ve dengeli beslenme ile koruyabileceğiniz sağlıklı görünümü göstermektedir.",
                                _ => ""
                            };

                            return (gorselData, hedefAciklama);
                        }
                    }

                    _gunluk.LogWarning($"DashScope yanıtı beklenmeyen formatta: {yanitIcerik}");
                }
                catch (Exception ex)
                {
                    _gunluk.LogError(ex, $"DashScope yanıtı parse edilemedi: {yanitIcerik}");
                }
            }
            else
            {
                _gunluk.LogWarning($"DashScope API hatası: {yanit.StatusCode} - {yanitIcerik}");
            }

            return (null, "Görsel dönüşüm şu anda yapılamadı. Lütfen tekrar deneyin.");
        }

        private async Task<string> GeminiGorselAnaliz(string base64Gorsel, string mimeType, HedefTipi hedef)
        {
            var apiAnahtari = _yapilandirma["YapayZekaAyarlari:GeminiApiKey"];

            if (string.IsNullOrEmpty(apiAnahtari))
            {
                return "API anahtarı yapılandırılmamış.";
            }

            var hedefAciklama = hedef switch
            {
                HedefTipi.KiloVermek => "daha zayıf, fit ve ince bir vücut yapısı",
                HedefTipi.KasYapmak => "daha kaslı, güçlü ve atletik bir vücut yapısı",
                HedefTipi.FormdaKalmak => "dengeli, sağlıklı ve formda bir vücut yapısı",
                _ => "sağlıklı bir vücut yapısı"
            };

            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiAnahtari}";

            var analizPrompt = $@"Bu fotoğraftaki kişiyi analiz et ve aşağıdaki bilgileri ver:

1. 📊 **Mevcut Durum Analizi:** Kişinin tahmini vücut tipi ve fiziksel durumu
2. 🎯 **Hedef:** '{hedefAciklama}' hedefine ulaşmak için neler yapılmalı
3. 🏋️ **Egzersiz Önerileri:** Bu hedefe yönelik spesifik egzersiz önerileri
4. 🥗 **Beslenme İpuçları:** Hedefe uygun beslenme önerileri
5. ⏰ **Tahmini Süre:** Bu hedefe ulaşmak için gereken tahmini süre

Türkçe, motive edici ve detaylı bir şekilde yanıt ver. Emoji kullan.";

            var istek = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = analizPrompt },
                            new 
                            { 
                                inline_data = new 
                                { 
                                    mime_type = mimeType,
                                    data = base64Gorsel
                                }
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 1500
                }
            };

            var json = JsonSerializer.Serialize(istek);
            var icerik = new StringContent(json, Encoding.UTF8, "application/json");

            var yanit = await httpClient.PostAsync(apiUrl, icerik);

            if (yanit.IsSuccessStatusCode)
            {
                var yanitJson = await yanit.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(yanitJson);

                var mesaj = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return mesaj ?? "Analiz yapılamadı.";
            }

            var hata = await yanit.Content.ReadAsStringAsync();
            _gunluk.LogError($"Gemini Vision hatası: {hata}");
            return "Görsel analizi şu anda yapılamadı.";
        }

        private async Task<string> GeminiMetinSorguGonder(string prompt)
        {
            var apiAnahtari = _yapilandirma["YapayZekaAyarlari:GeminiApiKey"];

            if (string.IsNullOrEmpty(apiAnahtari))
            {
                throw new InvalidOperationException("API anahtarı yapılandırılmamış");
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiAnahtari}";

            var istek = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.8,
                    maxOutputTokens = 2000
                }
            };

            var json = JsonSerializer.Serialize(istek);
            var icerik = new StringContent(json, Encoding.UTF8, "application/json");

            var yanit = await httpClient.PostAsync(apiUrl, icerik);

            if (yanit.IsSuccessStatusCode)
            {
                var yanitJson = await yanit.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(yanitJson);

                var mesaj = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return mesaj ?? "Öneri alınamadı.";
            }

            var hataDetay = await yanit.Content.ReadAsStringAsync();
            _gunluk.LogError($"Gemini API hatası: {yanit.StatusCode} - {hataDetay}");
            throw new Exception($"API hatası: {yanit.StatusCode}");
        }

        private string EgzersizPromptOlustur(EgzersizOneriViewModel model)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Sen deneyimli bir fitness antrenörü ve spor uzmanısın. Aşağıdaki kişiye özel bilgilere göre detaylı ve uygulanabilir bir haftalık egzersiz programı hazırla.");
            sb.AppendLine();
            sb.AppendLine("📋 KİŞİ BİLGİLERİ:");

            if (model.BoyCm.HasValue)
                sb.AppendLine($"• Boy: {model.BoyCm} cm");

            if (model.KiloKg.HasValue)
            {
                sb.AppendLine($"• Kilo: {model.KiloKg} kg");

                if (model.BoyCm.HasValue)
                {
                    var boyMetre = model.BoyCm.Value / 100.0;
                    var bmi = model.KiloKg.Value / (boyMetre * boyMetre);
                    sb.AppendLine($"• Hesaplanan BMI: {bmi:F1}");
                }
            }

            if (model.Yas.HasValue)
                sb.AppendLine($"• Yaş: {model.Yas}");

            if (model.Cinsiyet.HasValue)
            {
                var cinsiyetMetni = model.Cinsiyet switch
                {
                    Models.Cinsiyet.Erkek => "Erkek",
                    Models.Cinsiyet.Kadin => "Kadın",
                    _ => "Belirtilmemiş"
                };
                sb.AppendLine($"• Cinsiyet: {cinsiyetMetni}");
            }

            sb.AppendLine($"• Hedef: {HedefAciklamasiGetir(model.Hedef)}");
            sb.AppendLine($"• Mevcut Aktivite Seviyesi: {AktiviteSeviyesiAciklamasiGetir(model.AktiviteSeviye)}");

            if (!string.IsNullOrEmpty(model.EkBilgi))
                sb.AppendLine($"• Ek Bilgi/Sağlık Durumu: {model.EkBilgi}");

            sb.AppendLine();
            sb.AppendLine("📝 PROGRAM GEREKSİNİMLERİ:");
            sb.AppendLine("1. Pazartesi'den Pazar'a günlük detaylı program hazırla");
            sb.AppendLine("2. Her gün için hangi egzersizler yapılacağını belirt");
            sb.AppendLine("3. Set sayısı, tekrar sayısı ve dinlenme sürelerini yaz");
            sb.AppendLine("4. Isınma ve soğuma hareketlerini ekle");
            sb.AppendLine("5. Kişinin hedefine ve seviyesine uygun olsun");
            sb.AppendLine("6. Varsa sağlık durumunu dikkate al");
            sb.AppendLine("7. Motivasyon artırıcı ipuçları ekle");
            sb.AppendLine();
            sb.AppendLine("Programı Türkçe olarak, anlaşılır ve detaylı şekilde hazırla.");

            return sb.ToString();
        }

        private string HedefAciklamasiGetir(HedefTipi hedef)
        {
            return hedef switch
            {
                HedefTipi.KiloVermek => "Kilo vermek ve yağ yakmak",
                HedefTipi.KasYapmak => "Kas kütlesi artırmak",
                HedefTipi.FormdaKalmak => "Mevcut formu korumak ve sağlıklı kalmak",
                _ => "Genel sağlık ve fitness"
            };
        }

        private string AktiviteSeviyesiAciklamasiGetir(AktiviteSeviyesi seviye)
        {
            return seviye switch
            {
                AktiviteSeviyesi.Hareketsiz => "Hareketsiz - Masabaşı çalışan, egzersiz yapmıyor",
                AktiviteSeviyesi.AzAktif => "Az aktif - Haftada 1-2 gün hafif egzersiz",
                AktiviteSeviyesi.OrtaAktif => "Orta aktif - Haftada 3-4 gün düzenli egzersiz",
                AktiviteSeviyesi.CokAktif => "Çok aktif - Haftada 5-6 gün yoğun egzersiz",
                AktiviteSeviyesi.Profesyonel => "Profesyonel sporcu seviyesi",
                _ => "Belirtilmemiş"
            };
        }
    }
}
