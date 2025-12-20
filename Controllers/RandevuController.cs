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
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly UygulamaDbContext _veritabani;
        private readonly UserManager<Uye> _kullaniciYonetici;

        public RandevuController(UygulamaDbContext veritabani, UserManager<Uye> kullaniciYonetici)
        {
            _veritabani = veritabani;
            _kullaniciYonetici = kullaniciYonetici;
        }

        public async Task<IActionResult> Index()
        {
            var kullanici = await _kullaniciYonetici.GetUserAsync(User);
            if (kullanici == null) return RedirectToAction("Giris", "Hesap");

            var randevular = await _veritabani.Randevular
                .Include(r => r.Hizmet)
                .Include(r => r.Antrenor)
                .Where(r => r.UyeId == kullanici.Id)
                .OrderByDescending(r => r.Tarih)
                .ThenByDescending(r => r.BaslangicSaati)
                .ToListAsync();

            var model = new RandevuListeViewModel
            {
                Randevular = randevular.Select(r => new RandevuDetayViewModel
                {
                    Id = r.Id,
                    HizmetAdi = r.Hizmet?.Ad ?? "-",
                    AntrenorAdi = r.Antrenor?.TamAd ?? "-",
                    Tarih = r.Tarih,
                    BaslangicSaati = r.BaslangicSaati,
                    BitisSaati = r.BitisSaati,
                    Ucret = r.OdenenTutar,
                    Durum = r.Durum,
                    Notlar = r.Notlar,
                    OlusturulmaTarihi = r.OlusturulmaTarihi
                }).ToList(),
                ToplamRandevu = randevular.Count,
                BekleyenRandevu = randevular.Count(r => r.Durum == RandevuDurumu.Beklemede),
                TamamlananRandevu = randevular.Count(r => r.Durum == RandevuDurumu.Tamamlandi)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Al()
        {
            var model = new RandevuOlusturViewModel
            {
                Tarih = DateTime.Today.AddDays(1)
            };

            await FormSecenekleriniDoldur(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Al(RandevuOlusturViewModel model)
        {
            var kullanici = await _kullaniciYonetici.GetUserAsync(User);
            if (kullanici == null) return RedirectToAction("Giris", "Hesap");

            if (ModelState.IsValid)
            {
                if (!TimeSpan.TryParse(model.SecilenSaat, out TimeSpan baslangicSaati))
                {
                    ModelState.AddModelError("SecilenSaat", "Geçerli bir saat seçiniz.");
                    await FormSecenekleriniDoldur(model);
                    return View(model);
                }

                var hizmet = await _veritabani.Hizmetler.FindAsync(model.HizmetId);
                if (hizmet == null)
                {
                    ModelState.AddModelError("HizmetId", "Hizmet bulunamadı.");
                    await FormSecenekleriniDoldur(model);
                    return View(model);
                }

                var bitisSaati = baslangicSaati.Add(TimeSpan.FromMinutes(hizmet.SureDakika));

                var cakismaVar = await _veritabani.Randevular
                    .AnyAsync(r => r.AntrenorId == model.AntrenorId &&
                                   r.Tarih.Date == model.Tarih.Date &&
                                   r.Durum != RandevuDurumu.IptalEdildi &&
                                   ((baslangicSaati >= r.BaslangicSaati && baslangicSaati < r.BitisSaati) ||
                                    (bitisSaati > r.BaslangicSaati && bitisSaati <= r.BitisSaati) ||
                                    (baslangicSaati <= r.BaslangicSaati && bitisSaati >= r.BitisSaati)));

                if (cakismaVar)
                {
                    ModelState.AddModelError(string.Empty, "Seçtiğiniz saatte antrenörün başka bir randevusu var. Lütfen farklı bir saat seçiniz.");
                    await FormSecenekleriniDoldur(model);
                    return View(model);
                }

                var kullaniciCakisma = await _veritabani.Randevular
                    .AnyAsync(r => r.UyeId == kullanici.Id &&
                                   r.Tarih.Date == model.Tarih.Date &&
                                   r.Durum != RandevuDurumu.IptalEdildi &&
                                   ((baslangicSaati >= r.BaslangicSaati && baslangicSaati < r.BitisSaati) ||
                                    (bitisSaati > r.BaslangicSaati && bitisSaati <= r.BitisSaati)));

                if (kullaniciCakisma)
                {
                    ModelState.AddModelError(string.Empty, "Bu saatte zaten bir randevunuz var.");
                    await FormSecenekleriniDoldur(model);
                    return View(model);
                }

                var yeniRandevu = new Randevu
                {
                    UyeId = kullanici.Id,
                    AntrenorId = model.AntrenorId,
                    HizmetId = model.HizmetId,
                    Tarih = model.Tarih.Date,
                    BaslangicSaati = baslangicSaati,
                    BitisSaati = bitisSaati,
                    OdenenTutar = hizmet.Ucret,
                    Notlar = model.Notlar,
                    Durum = RandevuDurumu.Beklemede,
                    OlusturulmaTarihi = DateTime.Now
                };

                _veritabani.Randevular.Add(yeniRandevu);
                await _veritabani.SaveChangesAsync();

                TempData["Basari"] = "Randevunuz başarıyla oluşturuldu. Onay bekleniyor.";
                return RedirectToAction("Index");
            }

            await FormSecenekleriniDoldur(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Iptal(int id)
        {
            var kullanici = await _kullaniciYonetici.GetUserAsync(User);
            if (kullanici == null) return RedirectToAction("Giris", "Hesap");

            var randevu = await _veritabani.Randevular
                .FirstOrDefaultAsync(r => r.Id == id && r.UyeId == kullanici.Id);

            if (randevu == null)
            {
                TempData["Hata"] = "Randevu bulunamadı.";
                return RedirectToAction("Index");
            }

            if (randevu.Durum == RandevuDurumu.Tamamlandi)
            {
                TempData["Hata"] = "Tamamlanmış randevular iptal edilemez.";
                return RedirectToAction("Index");
            }

            var randevuZamani = randevu.Tarih.Date.Add(randevu.BaslangicSaati);
            if (randevuZamani < DateTime.Now.AddHours(24))
            {
                TempData["Hata"] = "Randevuya 24 saatten az kaldığı için iptal edilemez.";
                return RedirectToAction("Index");
            }

            randevu.Durum = RandevuDurumu.IptalEdildi;
            await _veritabani.SaveChangesAsync();

            TempData["Basari"] = "Randevunuz iptal edildi.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> HizmeteGoreAntrenorler(int hizmetId)
        {
            var antrenorler = await _veritabani.AntrenorHizmetleri
                .Include(ah => ah.Antrenor)
                .Where(ah => ah.HizmetId == hizmetId && ah.Antrenor!.AktifMi)
                .Select(ah => new { value = ah.AntrenorId, text = ah.Antrenor!.TamAd })
                .Distinct()
                .ToListAsync();

            return Json(antrenorler);
        }

        [HttpGet]
        public async Task<IActionResult> UygunSaatleriGetir(int antrenorId, DateTime tarih, int hizmetId)
        {
            var antrenor = await _veritabani.Antrenorler.FindAsync(antrenorId);
            var hizmet = await _veritabani.Hizmetler.FindAsync(hizmetId);

            if (antrenor == null || hizmet == null)
            {
                return Json(new List<object>());
            }

            var baslangic = antrenor.MesaiBaslangic;
            var bitis = antrenor.MesaiBitis;

            var mevcutRandevular = await _veritabani.Randevular
                .Where(r => r.AntrenorId == antrenorId &&
                            r.Tarih.Date == tarih.Date &&
                            r.Durum != RandevuDurumu.IptalEdildi)
                .Select(r => new { r.BaslangicSaati, r.BitisSaati })
                .ToListAsync();

            var uygunSaatler = new List<object>();
            var suanki = baslangic;
            var hizmetSuresi = TimeSpan.FromMinutes(hizmet.SureDakika);

            while (suanki.Add(hizmetSuresi) <= bitis)
            {
                var potansiyelBitis = suanki.Add(hizmetSuresi);

                var cakismaVar = mevcutRandevular.Any(r =>
                    (suanki >= r.BaslangicSaati && suanki < r.BitisSaati) ||
                    (potansiyelBitis > r.BaslangicSaati && potansiyelBitis <= r.BitisSaati) ||
                    (suanki <= r.BaslangicSaati && potansiyelBitis >= r.BitisSaati));

                var gecmis = tarih.Date == DateTime.Today && suanki < DateTime.Now.TimeOfDay;

                if (!cakismaVar && !gecmis)
                {
                    uygunSaatler.Add(new
                    {
                        value = suanki.ToString(@"hh\:mm"),
                        text = $"{suanki:hh\\:mm} - {potansiyelBitis:hh\\:mm}"
                    });
                }

                suanki = suanki.Add(TimeSpan.FromMinutes(30));
            }

            return Json(uygunSaatler);
        }

        private async Task FormSecenekleriniDoldur(RandevuOlusturViewModel model)
        {
            model.Hizmetler = await _veritabani.Hizmetler
                .Where(h => h.AktifMi)
                .Select(h => new SelectListItem
                {
                    Value = h.Id.ToString(),
                    Text = $"{h.Ad} ({h.SureDakika} dk - {h.Ucret:C})"
                })
                .ToListAsync();
        }
    }
}
