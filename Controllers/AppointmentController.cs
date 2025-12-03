using FitnessCenter.Data;
using FitnessCenter.Models;
using FitnessCenter.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Appointment
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new AppointmentListViewModel
                {
                    Id = a.Id,
                    TrainerName = a.Trainer!.FirstName + " " + a.Trainer.LastName,
                    ServiceName = a.Service!.Name,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    Price = a.Price,
                    Notes = a.Notes
                })
                .ToListAsync();

            return View(appointments);
        }

        // GET: /Appointment/Create
        public async Task<IActionResult> Create()
        {
            var model = new AppointmentCreateViewModel
            {
                Trainers = new SelectList(
                    await _context.Trainers.Where(t => t.IsActive).ToListAsync(),
                    "Id",
                    "FullName"),
                Services = new SelectList(
                    await _context.Services.Where(s => s.IsActive).ToListAsync(),
                    "Id",
                    "Name")
            };

            return View(model);
        }

        // POST: /Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var service = await _context.Services.FindAsync(model.ServiceId);
                
                if (service == null)
                {
                    ModelState.AddModelError("ServiceId", "Geçersiz hizmet seçimi.");
                    await LoadSelectLists(model);
                    return View(model);
                }

                // Saat bilgisini TimeSpan'e çevir
                if (!TimeSpan.TryParse(model.StartTime, out TimeSpan startTime))
                {
                    ModelState.AddModelError("StartTime", "Geçersiz saat formatı.");
                    await LoadSelectLists(model);
                    return View(model);
                }

                var endTime = startTime.Add(TimeSpan.FromMinutes(service.DurationMinutes));

                // Randevu çakışma kontrolü
                var hasConflict = await _context.Appointments
                    .AnyAsync(a =>
                        a.TrainerId == model.TrainerId &&
                        a.AppointmentDate == model.AppointmentDate &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.Rejected &&
                        ((a.StartTime <= startTime && a.EndTime > startTime) ||
                         (a.StartTime < endTime && a.EndTime >= endTime) ||
                         (a.StartTime >= startTime && a.EndTime <= endTime)));

                if (hasConflict)
                {
                    ModelState.AddModelError(string.Empty, "Seçilen tarih ve saatte antrenörün başka bir randevusu bulunmaktadır. Lütfen farklı bir saat seçiniz.");
                    await LoadSelectLists(model);
                    return View(model);
                }

                // Antrenör müsaitlik kontrolü
                var dayOfWeek = model.AppointmentDate.DayOfWeek;
                var isAvailable = await _context.TrainerAvailabilities
                    .AnyAsync(ta =>
                        ta.TrainerId == model.TrainerId &&
                        ta.DayOfWeek == dayOfWeek &&
                        ta.IsActive &&
                        ta.StartTime <= startTime &&
                        ta.EndTime >= endTime);

                if (!isAvailable)
                {
                    ModelState.AddModelError(string.Empty, "Antrenör seçilen tarih ve saatte müsait değildir.");
                    await LoadSelectLists(model);
                    return View(model);
                }

                var appointment = new Appointment
                {
                    UserId = userId!,
                    TrainerId = model.TrainerId,
                    ServiceId = model.ServiceId,
                    AppointmentDate = model.AppointmentDate,
                    StartTime = startTime,
                    EndTime = endTime,
                    Price = service.Price,
                    Notes = model.Notes,
                    Status = AppointmentStatus.Pending
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Randevunuz başarıyla oluşturuldu. Onay bekleniyor.";
                return RedirectToAction(nameof(Index));
            }

            await LoadSelectLists(model);
            return View(model);
        }

        // GET: /Appointment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var appointment = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: /Appointment/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Approved)
            {
                TempData["ErrorMessage"] = "Bu randevu iptal edilemez.";
                return RedirectToAction(nameof(Index));
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Randevunuz iptal edildi.";

            return RedirectToAction(nameof(Index));
        }

        // AJAX: /Appointment/GetAvailableTimes
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(int trainerId, int serviceId, DateTime date)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
            {
                return Json(new List<string>());
            }

            var dayOfWeek = date.DayOfWeek;
            var availability = await _context.TrainerAvailabilities
                .FirstOrDefaultAsync(ta =>
                    ta.TrainerId == trainerId &&
                    ta.DayOfWeek == dayOfWeek &&
                    ta.IsActive);

            if (availability == null)
            {
                return Json(new List<string>());
            }

            // Mevcut randevuları al
            var existingAppointments = await _context.Appointments
                .Where(a =>
                    a.TrainerId == trainerId &&
                    a.AppointmentDate == date &&
                    a.Status != AppointmentStatus.Cancelled &&
                    a.Status != AppointmentStatus.Rejected)
                .Select(a => new { a.StartTime, a.EndTime })
                .ToListAsync();

            var availableTimes = new List<string>();
            var currentTime = availability.StartTime;
            var serviceDuration = TimeSpan.FromMinutes(service.DurationMinutes);

            while (currentTime.Add(serviceDuration) <= availability.EndTime)
            {
                var endTime = currentTime.Add(serviceDuration);
                var hasConflict = existingAppointments.Any(a =>
                    (a.StartTime <= currentTime && a.EndTime > currentTime) ||
                    (a.StartTime < endTime && a.EndTime >= endTime) ||
                    (a.StartTime >= currentTime && a.EndTime <= endTime));

                if (!hasConflict)
                {
                    availableTimes.Add(currentTime.ToString(@"hh\:mm"));
                }

                currentTime = currentTime.Add(TimeSpan.FromMinutes(30)); // 30 dakikalık aralıklarla
            }

            return Json(availableTimes);
        }

        // AJAX: /Appointment/GetServicesByTrainer
        [HttpGet]
        public async Task<IActionResult> GetServicesByTrainer(int trainerId)
        {
            var services = await _context.TrainerServices
                .Where(ts => ts.TrainerId == trainerId)
                .Include(ts => ts.Service)
                .Select(ts => new { ts.Service!.Id, ts.Service.Name, ts.Service.Price, ts.Service.DurationMinutes })
                .ToListAsync();

            return Json(services);
        }

        private async Task LoadSelectLists(AppointmentCreateViewModel model)
        {
            model.Trainers = new SelectList(
                await _context.Trainers.Where(t => t.IsActive).ToListAsync(),
                "Id",
                "FullName",
                model.TrainerId);
            model.Services = new SelectList(
                await _context.Services.Where(s => s.IsActive).ToListAsync(),
                "Id",
                "Name",
                model.ServiceId);
        }
    }
}

