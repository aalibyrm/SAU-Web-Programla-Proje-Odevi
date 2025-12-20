using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonu.Data;
using SporSalonu.Models;

namespace SporSalonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly UygulamaDbContext _veritabani;

        public ApiController(UygulamaDbContext veritabani)
        {
            _veritabani = veritabani;
        }

        [HttpGet("antrenorler")]
        public async Task<ActionResult<IEnumerable<object>>> TumAntrenorleriGetir()
        {
            var antrenorler = await _veritabani.Antrenorler
                .Where(a => a.AktifMi)
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .Select(a => new
                {
                    a.Id,
                    TamAd = a.Ad + " " + a.Soyad,
                    a.Eposta,
                    a.Telefon,
                    a.UzmanlikAlanlari,
                    MesaiBaslangic = a.MesaiBaslangic.ToString(@"hh\:mm"),
                    MesaiBitis = a.MesaiBitis.ToString(@"hh\:mm"),
                    SalonAdi = a.Salon != null ? a.Salon.Ad : "-",
                    Hizmetler = a.AntrenorHizmetleri
                        .Where(ah => ah.Hizmet != null)
                        .Select(ah => ah.Hizmet!.Ad)
                        .ToList()
                })
                .ToListAsync();

            return Ok(antrenorler);
        }

        [HttpGet("musait-antrenorler")]
        public async Task<ActionResult<IEnumerable<object>>> MusaitAntrenorleriGetir(
            [FromQuery] DateTime tarih,
            [FromQuery] string? saat = null)
        {
            TimeSpan? kontrolSaati = null;
            if (!string.IsNullOrEmpty(saat) && TimeSpan.TryParse(saat, out TimeSpan parsedSaat))
            {
                kontrolSaati = parsedSaat;
            }

            var doluRandevular = await _veritabani.Randevular
                .Where(r => r.Tarih.Date == tarih.Date && r.Durum != RandevuDurumu.IptalEdildi)
                .Select(r => new { r.AntrenorId, r.BaslangicSaati, r.BitisSaati })
                .ToListAsync();

            var antrenorler = await _veritabani.Antrenorler
                .Where(a => a.AktifMi)
                .Select(a => new
                {
                    a.Id,
                    TamAd = a.Ad + " " + a.Soyad,
                    a.UzmanlikAlanlari,
                    a.MesaiBaslangic,
                    a.MesaiBitis
                })
                .ToListAsync();

            var musaitAntrenorler = antrenorler
                .Where(a =>
                {
                    if (kontrolSaati.HasValue)
                    {
                        if (kontrolSaati.Value < a.MesaiBaslangic || kontrolSaati.Value >= a.MesaiBitis)
                            return false;

                        var mesgulMu = doluRandevular.Any(r =>
                            r.AntrenorId == a.Id &&
                            kontrolSaati.Value >= r.BaslangicSaati &&
                            kontrolSaati.Value < r.BitisSaati);

                        return !mesgulMu;
                    }

                    var antrenorRandevuSayisi = doluRandevular.Count(r => r.AntrenorId == a.Id);
                    var maxRandevu = (a.MesaiBitis - a.MesaiBaslangic).TotalHours;
                    return antrenorRandevuSayisi < maxRandevu;
                })
                .Select(a => new
                {
                    a.Id,
                    a.TamAd,
                    a.UzmanlikAlanlari,
                    MesaiBaslangic = a.MesaiBaslangic.ToString(@"hh\:mm"),
                    MesaiBitis = a.MesaiBitis.ToString(@"hh\:mm")
                })
                .ToList();

            return Ok(musaitAntrenorler);
        }

        [HttpGet("hizmetler")]
        public async Task<ActionResult<IEnumerable<object>>> TumHizmetleriGetir()
        {
            var hizmetler = await _veritabani.Hizmetler
                .Where(h => h.AktifMi)
                .Select(h => new
                {
                    h.Id,
                    h.Ad,
                    h.Aciklama,
                    h.SureDakika,
                    Ucret = h.Ucret.ToString("C", new System.Globalization.CultureInfo("tr-TR")),
                    AntrenorSayisi = h.AntrenorHizmetleri.Count(ah => ah.Antrenor != null && ah.Antrenor.AktifMi)
                })
                .ToListAsync();

            return Ok(hizmetler);
        }

        [HttpGet("uye-randevulari/{uyeId}")]
        public async Task<ActionResult<IEnumerable<object>>> UyeRandevulariniGetir(string uyeId)
        {
            var randevular = await _veritabani.Randevular
                .Where(r => r.UyeId == uyeId)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.Tarih)
                .Select(r => new
                {
                    r.Id,
                    Tarih = r.Tarih.ToString("dd.MM.yyyy"),
                    BaslangicSaati = r.BaslangicSaati.ToString(@"hh\:mm"),
                    BitisSaati = r.BitisSaati.ToString(@"hh\:mm"),
                    HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : "-",
                    AntrenorAdi = r.Antrenor != null ? r.Antrenor.Ad + " " + r.Antrenor.Soyad : "-",
                    Durum = r.Durum.ToString(),
                    Ucret = r.OdenenTutar.ToString("C", new System.Globalization.CultureInfo("tr-TR"))
                })
                .ToListAsync();

            return Ok(randevular);
        }

        [HttpGet("randevular")]
        public async Task<ActionResult<IEnumerable<object>>> RandevulariFiltrele(
            [FromQuery] DateTime? baslangic,
            [FromQuery] DateTime? bitis,
            [FromQuery] int? antrenorId,
            [FromQuery] int? hizmetId)
        {
            var sorgu = _veritabani.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .AsQueryable();

            if (baslangic.HasValue)
                sorgu = sorgu.Where(r => r.Tarih >= baslangic.Value.Date);

            if (bitis.HasValue)
                sorgu = sorgu.Where(r => r.Tarih <= bitis.Value.Date);

            if (antrenorId.HasValue)
                sorgu = sorgu.Where(r => r.AntrenorId == antrenorId.Value);

            if (hizmetId.HasValue)
                sorgu = sorgu.Where(r => r.HizmetId == hizmetId.Value);

            var randevular = await sorgu
                .OrderByDescending(r => r.Tarih)
                .ThenBy(r => r.BaslangicSaati)
                .Select(r => new
                {
                    r.Id,
                    UyeAdi = r.Uye != null ? r.Uye.Ad + " " + r.Uye.Soyad : "-",
                    AntrenorAdi = r.Antrenor != null ? r.Antrenor.Ad + " " + r.Antrenor.Soyad : "-",
                    HizmetAdi = r.Hizmet != null ? r.Hizmet.Ad : "-",
                    Tarih = r.Tarih.ToString("dd.MM.yyyy"),
                    Saat = r.BaslangicSaati.ToString(@"hh\:mm") + " - " + r.BitisSaati.ToString(@"hh\:mm"),
                    Durum = r.Durum.ToString(),
                    r.OdenenTutar
                })
                .Take(100)
                .ToListAsync();

            return Ok(randevular);
        }

        [HttpGet("istatistikler")]
        public async Task<ActionResult<object>> IstatistikleriGetir()
        {
            var istatistikler = new
            {
                ToplamAntrenor = await _veritabani.Antrenorler.CountAsync(a => a.AktifMi),
                ToplamHizmet = await _veritabani.Hizmetler.CountAsync(h => h.AktifMi),
                ToplamRandevu = await _veritabani.Randevular.CountAsync(),
                BekleyenRandevu = await _veritabani.Randevular.CountAsync(r => r.Durum == RandevuDurumu.Beklemede),
                TamamlananRandevu = await _veritabani.Randevular.CountAsync(r => r.Durum == RandevuDurumu.Tamamlandi),
                BugunkuRandevu = await _veritabani.Randevular.CountAsync(r => r.Tarih.Date == DateTime.Today),
                ToplamGelir = await _veritabani.Randevular
                    .Where(r => r.Durum == RandevuDurumu.Tamamlandi)
                    .SumAsync(r => r.OdenenTutar),
                BuAykiGelir = await _veritabani.Randevular
                    .Where(r => r.Durum == RandevuDurumu.Tamamlandi &&
                               r.Tarih.Month == DateTime.Today.Month &&
                               r.Tarih.Year == DateTime.Today.Year)
                    .SumAsync(r => r.OdenenTutar)
            };

            return Ok(istatistikler);
        }
    }
}
