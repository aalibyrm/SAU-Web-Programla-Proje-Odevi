using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporSalonu.Data;
using SporSalonu.Models;
using SporSalonu.Models.ViewModels;

namespace SporSalonu.Controllers
{
    [Authorize(Roles = "Admin")]
    public class YonetimController : Controller
    {
        private readonly UygulamaDbContext _veritabani;
        private readonly UserManager<Uye> _kullaniciYonetici;

        public YonetimController(UygulamaDbContext veritabani, UserManager<Uye> kullaniciYonetici)
        {
            _veritabani = veritabani;
            _kullaniciYonetici = kullaniciYonetici;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ToplamUye = await _kullaniciYonetici.Users.CountAsync();
            ViewBag.ToplamAntrenor = await _veritabani.Antrenorler.CountAsync();
            ViewBag.ToplamHizmet = await _veritabani.Hizmetler.CountAsync();
            ViewBag.BekleyenRandevu = await _veritabani.Randevular.CountAsync(r => r.Durum == RandevuDurumu.Beklemede);
            ViewBag.BugunkuRandevu = await _veritabani.Randevular.CountAsync(r => r.Tarih.Date == DateTime.Today);

            return View();
        }

        public async Task<IActionResult> Hizmetler()
        {
            var hizmetler = await _veritabani.Hizmetler
                .Include(h => h.Salon)
                .OrderBy(h => h.Ad)
                .ToListAsync();
            return View(hizmetler);
        }

        [HttpGet]
        public async Task<IActionResult> HizmetEkle()
        {
            await SalonListesiniYukle();
            return View(new Hizmet());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetEkle(Hizmet model)
        {
            if (ModelState.IsValid)
            {
                _veritabani.Hizmetler.Add(model);
                await _veritabani.SaveChangesAsync();
                TempData["Basari"] = "Hizmet başarıyla eklendi.";
                return RedirectToAction("Hizmetler");
            }
            await SalonListesiniYukle();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> HizmetDuzenle(int id)
        {
            var hizmet = await _veritabani.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();

            await SalonListesiniYukle();
            return View(hizmet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetDuzenle(int id, Hizmet model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _veritabani.Update(model);
                await _veritabani.SaveChangesAsync();
                TempData["Basari"] = "Hizmet güncellendi.";
                return RedirectToAction("Hizmetler");
            }
            await SalonListesiniYukle();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HizmetSil(int id)
        {
            var hizmet = await _veritabani.Hizmetler.FindAsync(id);
            if (hizmet != null)
            {
                var iliskiliRandevu = await _veritabani.Randevular.AnyAsync(r => r.HizmetId == id);
                if (iliskiliRandevu)
                {
                    TempData["Hata"] = "Bu hizmete ait randevular bulunduğu için silinemez.";
                    return RedirectToAction("Hizmetler");
                }

                _veritabani.Hizmetler.Remove(hizmet);
                await _veritabani.SaveChangesAsync();
                TempData["Basari"] = "Hizmet silindi.";
            }
            return RedirectToAction("Hizmetler");
        }

        public async Task<IActionResult> Antrenorler()
        {
            var antrenorler = await _veritabani.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .OrderBy(a => a.Ad)
                .ToListAsync();
            return View(antrenorler);
        }

        [HttpGet]
        public async Task<IActionResult> AntrenorEkle()
        {
            await SalonListesiniYukle();
            await HizmetListesiniYukle();
            return View(new Antrenor());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AntrenorEkle(Antrenor model, int[] secilenHizmetler)
        {
            if (ModelState.IsValid)
            {
                _veritabani.Antrenorler.Add(model);
                await _veritabani.SaveChangesAsync();

                if (secilenHizmetler != null)
                {
                    foreach (var hizmetId in secilenHizmetler)
                    {
                        _veritabani.AntrenorHizmetleri.Add(new AntrenorHizmet
                        {
                            AntrenorId = model.Id,
                            HizmetId = hizmetId
                        });
                    }
                    await _veritabani.SaveChangesAsync();
                }

                TempData["Basari"] = "Antrenör başarıyla eklendi.";
                return RedirectToAction("Antrenorler");
            }
            await SalonListesiniYukle();
            await HizmetListesiniYukle();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AntrenorDuzenle(int id)
        {
            var antrenor = await _veritabani.Antrenorler
                .Include(a => a.AntrenorHizmetleri)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (antrenor == null) return NotFound();

            await SalonListesiniYukle();
            await HizmetListesiniYukle();
            ViewBag.SecilenHizmetler = antrenor.AntrenorHizmetleri.Select(ah => ah.HizmetId).ToList();
            return View(antrenor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AntrenorDuzenle(int id, Antrenor model, int[] secilenHizmetler)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _veritabani.Update(model);

                var mevcutHizmetler = await _veritabani.AntrenorHizmetleri
                    .Where(ah => ah.AntrenorId == id)
                    .ToListAsync();
                _veritabani.AntrenorHizmetleri.RemoveRange(mevcutHizmetler);

                if (secilenHizmetler != null)
                {
                    foreach (var hizmetId in secilenHizmetler)
                    {
                        _veritabani.AntrenorHizmetleri.Add(new AntrenorHizmet
                        {
                            AntrenorId = model.Id,
                            HizmetId = hizmetId
                        });
                    }
                }

                await _veritabani.SaveChangesAsync();
                TempData["Basari"] = "Antrenör güncellendi.";
                return RedirectToAction("Antrenorler");
            }
            await SalonListesiniYukle();
            await HizmetListesiniYukle();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AntrenorSil(int id)
        {
            var antrenor = await _veritabani.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                var iliskiliRandevu = await _veritabani.Randevular.AnyAsync(r => r.AntrenorId == id);
                if (iliskiliRandevu)
                {
                    TempData["Hata"] = "Bu antrenöre ait randevular bulunduğu için silinemez.";
                    return RedirectToAction("Antrenorler");
                }

                _veritabani.Antrenorler.Remove(antrenor);
                await _veritabani.SaveChangesAsync();
                TempData["Basari"] = "Antrenör silindi.";
            }
            return RedirectToAction("Antrenorler");
        }

        public async Task<IActionResult> Randevular(string durum = "")
        {
            var sorgu = _veritabani.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .AsQueryable();

            if (!string.IsNullOrEmpty(durum) && Enum.TryParse<RandevuDurumu>(durum, out var seciliDurum))
            {
                sorgu = sorgu.Where(r => r.Durum == seciliDurum);
            }

            var randevular = await sorgu
                .OrderByDescending(r => r.Tarih)
                .ThenByDescending(r => r.BaslangicSaati)
                .ToListAsync();

            ViewBag.SeciliDurum = durum;
            return View(randevular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuOnayla(int id)
        {
            var randevu = await _veritabani.Randevular.FindAsync(id);
            if (randevu != null && randevu.Durum == RandevuDurumu.Beklemede)
            {
                randevu.Durum = RandevuDurumu.Onaylandi;
                await _veritabani.SaveChangesAsync();
                TempData["Basari"] = "Randevu onaylandı.";
            }
            return RedirectToAction("Randevular");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuIptal(int id)
        {
            var randevu = await _veritabani.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = RandevuDurumu.IptalEdildi;
                await _veritabani.SaveChangesAsync();
                TempData["Basari"] = "Randevu iptal edildi.";
            }
            return RedirectToAction("Randevular");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuTamamla(int id)
        {
            var randevu = await _veritabani.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = RandevuDurumu.Tamamlandi;
                await _veritabani.SaveChangesAsync();
                TempData["Basari"] = "Randevu tamamlandı olarak işaretlendi.";
            }
            return RedirectToAction("Randevular");
        }

        public async Task<IActionResult> Uyeler()
        {
            var uyeler = await _kullaniciYonetici.Users
                .OrderByDescending(u => u.KayitTarihi)
                .ToListAsync();
            return View(uyeler);
        }

        [HttpGet]
        public IActionResult UyeEkle()
        {
            return View(new UyeEkleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UyeEkle(UyeEkleViewModel model)
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
                    BoyCm = model.BoyCm,
                    KiloKg = model.KiloKg,
                    DogumTarihi = model.DogumTarihi,
                    Cinsiyet = model.Cinsiyet,
                    KayitTarihi = DateTime.Now,
                    EmailConfirmed = true
                };

                var sonuc = await _kullaniciYonetici.CreateAsync(yeniUye, model.Sifre);

                if (sonuc.Succeeded)
                {
                    await _kullaniciYonetici.AddToRoleAsync(yeniUye, "Uye");
                    TempData["Basari"] = "Üye başarıyla eklendi.";
                    return RedirectToAction("Uyeler");
                }

                foreach (var hata in sonuc.Errors)
                {
                    ModelState.AddModelError(string.Empty, hata.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> UyeDuzenle(string id)
        {
            var uye = await _kullaniciYonetici.FindByIdAsync(id);
            if (uye == null) return NotFound();

            var model = new UyeDuzenleViewModel
            {
                Id = uye.Id,
                Ad = uye.Ad,
                Soyad = uye.Soyad,
                Eposta = uye.Email ?? "",
                Telefon = uye.PhoneNumber,
                BoyCm = uye.BoyCm,
                KiloKg = uye.KiloKg,
                DogumTarihi = uye.DogumTarihi,
                Cinsiyet = uye.Cinsiyet
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UyeDuzenle(UyeDuzenleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var uye = await _kullaniciYonetici.FindByIdAsync(model.Id);
                if (uye == null) return NotFound();

                uye.Ad = model.Ad;
                uye.Soyad = model.Soyad;
                uye.Email = model.Eposta;
                uye.UserName = model.Eposta;
                uye.PhoneNumber = model.Telefon;
                uye.BoyCm = model.BoyCm;
                uye.KiloKg = model.KiloKg;
                uye.DogumTarihi = model.DogumTarihi;
                uye.Cinsiyet = model.Cinsiyet;

                var sonuc = await _kullaniciYonetici.UpdateAsync(uye);

                if (sonuc.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.YeniSifre))
                    {
                        var token = await _kullaniciYonetici.GeneratePasswordResetTokenAsync(uye);
                        var sifreSonuc = await _kullaniciYonetici.ResetPasswordAsync(uye, token, model.YeniSifre);
                        
                        if (!sifreSonuc.Succeeded)
                        {
                            foreach (var hata in sifreSonuc.Errors)
                            {
                                ModelState.AddModelError(string.Empty, hata.Description);
                            }
                            return View(model);
                        }
                    }

                    TempData["Basari"] = "Üye bilgileri güncellendi.";
                    return RedirectToAction("Uyeler");
                }

                foreach (var hata in sonuc.Errors)
                {
                    ModelState.AddModelError(string.Empty, hata.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UyeSil(string id)
        {
            var uye = await _kullaniciYonetici.FindByIdAsync(id);
            if (uye == null)
            {
                TempData["Hata"] = "Üye bulunamadı.";
                return RedirectToAction("Uyeler");
            }

            if (await _kullaniciYonetici.IsInRoleAsync(uye, "Admin"))
            {
                TempData["Hata"] = "Admin kullanıcı silinemez.";
                return RedirectToAction("Uyeler");
            }

            var randevuVar = await _veritabani.Randevular.AnyAsync(r => r.UyeId == id);
            if (randevuVar)
            {
                TempData["Hata"] = "Bu üyeye ait randevular bulunduğu için silinemez. Önce randevuları silin.";
                return RedirectToAction("Uyeler");
            }

            var sonuc = await _kullaniciYonetici.DeleteAsync(uye);
            if (sonuc.Succeeded)
            {
                TempData["Basari"] = "Üye başarıyla silindi.";
            }
            else
            {
                TempData["Hata"] = "Üye silinirken bir hata oluştu.";
            }

            return RedirectToAction("Uyeler");
        }

        private async Task SalonListesiniYukle()
        {
            ViewBag.Salonlar = new SelectList(
                await _veritabani.Salonlar.Where(s => s.AktifMi).ToListAsync(),
                "Id", "Ad");
        }

        private async Task HizmetListesiniYukle()
        {
            ViewBag.Hizmetler = await _veritabani.Hizmetler
                .Where(h => h.AktifMi)
                .Select(h => new SelectListItem { Value = h.Id.ToString(), Text = h.Ad })
                .ToListAsync();
        }
    }
}
