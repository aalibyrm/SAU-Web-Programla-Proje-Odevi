using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SporSalonu.Models;
using SporSalonu.Models.ViewModels;

namespace SporSalonu.Controllers
{
    public class HesapController : Controller
    {
        private readonly UserManager<Uye> _kullaniciYonetici;
        private readonly SignInManager<Uye> _oturumYonetici;

        public HesapController(UserManager<Uye> kullaniciYonetici, SignInManager<Uye> oturumYonetici)
        {
            _kullaniciYonetici = kullaniciYonetici;
            _oturumYonetici = oturumYonetici;
        }

        [HttpGet]
        public IActionResult Giris(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giris(GirisViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var kullanici = await _kullaniciYonetici.FindByEmailAsync(model.Eposta);
                if (kullanici != null)
                {
                    var sonuc = await _oturumYonetici.PasswordSignInAsync(
                        kullanici.UserName!,
                        model.Sifre,
                        model.BeniHatirla,
                        lockoutOnFailure: true);

                    if (sonuc.Succeeded)
                    {
                        if (await _kullaniciYonetici.IsInRoleAsync(kullanici, "Admin"))
                        {
                            return RedirectToAction("Index", "Yonetim");
                        }

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }

                    if (sonuc.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Hesabınız geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.");
                        return View(model);
                    }
                }

                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Kayit()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kayit(KayitViewModel model)
        {
            if (ModelState.IsValid)
            {
                var yeniUye = new Uye
                {
                    UserName = model.Eposta,
                    Email = model.Eposta,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    PhoneNumber = model.Telefon,
                    EmailConfirmed = true,
                    KayitTarihi = DateTime.Now
                };

                var sonuc = await _kullaniciYonetici.CreateAsync(yeniUye, model.Sifre);

                if (sonuc.Succeeded)
                {
                    await _kullaniciYonetici.AddToRoleAsync(yeniUye, "Uye");
                    await _oturumYonetici.SignInAsync(yeniUye, isPersistent: false);

                    TempData["Basari"] = "Kayıt başarılı! Hoş geldiniz.";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var hata in sonuc.Errors)
                {
                    ModelState.AddModelError(string.Empty, HataMesajiniCevir(hata.Code));
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis()
        {
            await _oturumYonetici.SignOutAsync();
            TempData["Bilgi"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            var kullanici = await _kullaniciYonetici.GetUserAsync(User);
            if (kullanici == null)
            {
                return RedirectToAction("Giris");
            }

            var model = new ProfilDuzenleViewModel
            {
                Ad = kullanici.Ad,
                Soyad = kullanici.Soyad,
                Telefon = kullanici.PhoneNumber,
                DogumTarihi = kullanici.DogumTarihi,
                BoyCm = kullanici.BoyCm,
                KiloKg = kullanici.KiloKg,
                Cinsiyet = kullanici.Cinsiyet
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profil(ProfilDuzenleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var kullanici = await _kullaniciYonetici.GetUserAsync(User);
                if (kullanici == null)
                {
                    return RedirectToAction("Giris");
                }

                kullanici.Ad = model.Ad;
                kullanici.Soyad = model.Soyad;
                kullanici.PhoneNumber = model.Telefon;
                kullanici.DogumTarihi = model.DogumTarihi;
                kullanici.BoyCm = model.BoyCm;
                kullanici.KiloKg = model.KiloKg;
                kullanici.Cinsiyet = model.Cinsiyet;

                var sonuc = await _kullaniciYonetici.UpdateAsync(kullanici);
                if (sonuc.Succeeded)
                {
                    TempData["Basari"] = "Profil bilgileriniz güncellendi.";
                    return RedirectToAction("Profil");
                }

                foreach (var hata in sonuc.Errors)
                {
                    ModelState.AddModelError(string.Empty, hata.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ErisimEngellendi()
        {
            return View();
        }

        private string HataMesajiniCevir(string hatakodu)
        {
            return hatakodu switch
            {
                "DuplicateEmail" => "Bu e-posta adresi zaten kayıtlı.",
                "DuplicateUserName" => "Bu kullanıcı adı zaten kullanılıyor.",
                "PasswordTooShort" => "Şifre çok kısa. En az 6 karakter olmalı.",
                "PasswordRequiresDigit" => "Şifre en az bir rakam içermelidir.",
                "PasswordRequiresLower" => "Şifre en az bir küçük harf içermelidir.",
                "PasswordRequiresUpper" => "Şifre en az bir büyük harf içermelidir.",
                _ => "Bir hata oluştu. Lütfen tekrar deneyin."
            };
        }
    }
}
