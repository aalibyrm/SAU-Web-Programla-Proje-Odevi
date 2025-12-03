using FitnessCenter.Data;
using FitnessCenter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Trainers
        public async Task<IActionResult> Index()
        {
            var trainers = await _context.Trainers
                .Include(t => t.Gym)
                .OrderBy(t => t.FirstName)
                .ToListAsync();
            return View(trainers);
        }

        // GET: Admin/Trainers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .Include(t => t.TrainerServices!)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.Availabilities)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // GET: Admin/Trainers/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Gyms = new SelectList(await _context.Gyms.ToListAsync(), "Id", "Name");
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            return View();
        }

        // POST: Admin/Trainers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Trainer trainer, int[] SelectedServices)
        {
            if (ModelState.IsValid)
            {
                _context.Trainers.Add(trainer);
                await _context.SaveChangesAsync();

                // Hizmetleri ekle
                if (SelectedServices != null)
                {
                    foreach (var serviceId in SelectedServices)
                    {
                        _context.TrainerServices.Add(new TrainerService
                        {
                            TrainerId = trainer.Id,
                            ServiceId = serviceId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Antrenör başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Gyms = new SelectList(await _context.Gyms.ToListAsync(), "Id", "Name", trainer.GymId);
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            return View(trainer);
        }

        // GET: Admin/Trainers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
            {
                return NotFound();
            }

            ViewBag.Gyms = new SelectList(await _context.Gyms.ToListAsync(), "Id", "Name", trainer.GymId);
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            ViewBag.SelectedServices = trainer.TrainerServices?.Select(ts => ts.ServiceId).ToList() ?? new List<int>();
            return View(trainer);
        }

        // POST: Admin/Trainers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Trainer trainer, int[] SelectedServices)
        {
            if (id != trainer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);

                    // Mevcut hizmetleri kaldır
                    var existingServices = await _context.TrainerServices
                        .Where(ts => ts.TrainerId == trainer.Id)
                        .ToListAsync();
                    _context.TrainerServices.RemoveRange(existingServices);

                    // Yeni hizmetleri ekle
                    if (SelectedServices != null)
                    {
                        foreach (var serviceId in SelectedServices)
                        {
                            _context.TrainerServices.Add(new TrainerService
                            {
                                TrainerId = trainer.Id,
                                ServiceId = serviceId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Antrenör başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Gyms = new SelectList(await _context.Gyms.ToListAsync(), "Id", "Name", trainer.GymId);
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            return View(trainer);
        }

        // GET: Admin/Trainers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.Gym)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // POST: Admin/Trainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                trainer.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Antrenör başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }
    }
}

