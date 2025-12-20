using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonu.Data;
using SporSalonu.Models;
using System.Diagnostics;

namespace SporSalonu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _gunluk;
        private readonly UygulamaDbContext _veritabani;

        public HomeController(ILogger<HomeController> gunluk, UygulamaDbContext veritabani)
        {
            _gunluk = gunluk;
            _veritabani = veritabani;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Salon = await _veritabani.Salonlar.FirstOrDefaultAsync(s => s.AktifMi);
            ViewBag.Hizmetler = await _veritabani.Hizmetler
                .Where(h => h.AktifMi)
                .Take(6)
                .ToListAsync();
            ViewBag.Antrenorler = await _veritabani.Antrenorler
                .Where(a => a.AktifMi)
                .Take(3)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> Hizmetler()
        {
            var hizmetler = await _veritabani.Hizmetler
                .Where(h => h.AktifMi)
                .Include(h => h.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Antrenor)
                .ToListAsync();

            return View(hizmetler);
        }

        public async Task<IActionResult> Antrenorler()
        {
            var antrenorler = await _veritabani.Antrenorler
                .Where(a => a.AktifMi)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .ToListAsync();

            return View(antrenorler);
        }

        public async Task<IActionResult> Iletisim()
        {
            ViewBag.Salon = await _veritabani.Salonlar.FirstOrDefaultAsync(s => s.AktifMi);
            return View();
        }

        public async Task<IActionResult> Hakkimizda()
        {
            ViewBag.Salon = await _veritabani.Salonlar.FirstOrDefaultAsync(s => s.AktifMi);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
