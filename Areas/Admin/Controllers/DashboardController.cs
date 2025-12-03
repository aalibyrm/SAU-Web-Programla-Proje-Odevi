using FitnessCenter.Data;
using FitnessCenter.Models;
using FitnessCenter.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var model = new AdminDashboardViewModel
            {
                TotalMembers = await _userManager.GetUsersInRoleAsync("Member").ContinueWith(t => t.Result.Count),
                TotalTrainers = await _context.Trainers.CountAsync(t => t.IsActive),
                TotalServices = await _context.Services.CountAsync(s => s.IsActive),
                TotalAppointments = await _context.Appointments.CountAsync(),
                PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending),
                TodayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate == today),
                TotalRevenue = await _context.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .SumAsync(a => a.Price),
                MonthlyRevenue = await _context.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed && a.AppointmentDate >= startOfMonth)
                    .SumAsync(a => a.Price),
                RecentAppointments = await _context.Appointments
                    .Include(a => a.Trainer)
                    .Include(a => a.Service)
                    .Include(a => a.User)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .Select(a => new AppointmentListViewModel
                    {
                        Id = a.Id,
                        TrainerName = a.Trainer!.FirstName + " " + a.Trainer.LastName,
                        ServiceName = a.Service!.Name,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Status = a.Status,
                        Price = a.Price
                    })
                    .ToListAsync(),
                TopTrainers = await _context.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .GroupBy(a => new { a.TrainerId, a.Trainer!.FirstName, a.Trainer.LastName })
                    .Select(g => new TrainerStatsViewModel
                    {
                        TrainerId = g.Key.TrainerId,
                        TrainerName = g.Key.FirstName + " " + g.Key.LastName,
                        TotalAppointments = g.Count(),
                        TotalRevenue = g.Sum(a => a.Price)
                    })
                    .OrderByDescending(x => x.TotalAppointments)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}

