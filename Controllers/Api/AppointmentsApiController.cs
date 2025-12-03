using FitnessCenter.Data;
using FitnessCenter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AppointmentsApi
        // Tüm randevuları listele (Admin için)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? trainerId,
            [FromQuery] AppointmentStatus? status)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .AsQueryable();

            // LINQ ile filtreleme
            if (startDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate <= endDate.Value);
            }

            if (trainerId.HasValue)
            {
                query = query.Where(a => a.TrainerId == trainerId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    UserName = a.User != null ? a.User.FirstName + " " + a.User.LastName : "Bilinmiyor",
                    UserEmail = a.User != null ? a.User.Email : null,
                    TrainerName = a.Trainer != null ? a.Trainer.FirstName + " " + a.Trainer.LastName : "Bilinmiyor",
                    ServiceName = a.Service != null ? a.Service.Name : "Bilinmiyor",
                    a.AppointmentDate,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    Status = a.Status.ToString(),
                    a.Price,
                    a.Notes,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = appointments.Count,
                Appointments = appointments
            });
        }

        // GET: api/AppointmentsApi/User/{userId}
        // Belirli bir üyenin randevularını getir
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserAppointments(string userId)
        {
            var appointments = await _context.Appointments
                .Where(a => a.UserId == userId)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    TrainerName = a.Trainer != null ? a.Trainer.FirstName + " " + a.Trainer.LastName : "Bilinmiyor",
                    ServiceName = a.Service != null ? a.Service.Name : "Bilinmiyor",
                    a.AppointmentDate,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    Status = a.Status.ToString(),
                    a.Price,
                    a.Notes
                })
                .ToListAsync();

            return Ok(new
            {
                UserId = userId,
                TotalCount = appointments.Count,
                Appointments = appointments
            });
        }

        // GET: api/AppointmentsApi/Trainer/{trainerId}
        // Belirli bir antrenörün randevularını getir
        [HttpGet("Trainer/{trainerId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainerAppointments(
            int trainerId,
            [FromQuery] DateTime? date)
        {
            var query = _context.Appointments
                .Where(a => a.TrainerId == trainerId)
                .Where(a => a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Rejected)
                .Include(a => a.User)
                .Include(a => a.Service)
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(a => a.AppointmentDate == date.Value);
            }

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    UserName = a.User != null ? a.User.FirstName + " " + a.User.LastName : "Bilinmiyor",
                    ServiceName = a.Service != null ? a.Service.Name : "Bilinmiyor",
                    a.AppointmentDate,
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    Status = a.Status.ToString()
                })
                .ToListAsync();

            return Ok(new
            {
                TrainerId = trainerId,
                FilterDate = date?.ToString("yyyy-MM-dd"),
                TotalCount = appointments.Count,
                Appointments = appointments
            });
        }

        // GET: api/AppointmentsApi/Statistics
        // Randevu istatistiklerini getir
        [HttpGet("Statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var statistics = new
            {
                TotalAppointments = await _context.Appointments.CountAsync(),
                TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate == today),
                MonthlyAppointments = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate >= startOfMonth && a.AppointmentDate <= endOfMonth),
                PendingAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Pending),
                ApprovedAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Approved),
                CompletedAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Completed),
                CancelledAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == AppointmentStatus.Cancelled),
                TotalRevenue = await _context.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .SumAsync(a => a.Price),
                MonthlyRevenue = await _context.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed &&
                               a.AppointmentDate >= startOfMonth &&
                               a.AppointmentDate <= endOfMonth)
                    .SumAsync(a => a.Price),
                TopTrainers = await _context.Appointments
                    .Where(a => a.Status == AppointmentStatus.Completed)
                    .GroupBy(a => new { a.TrainerId, a.Trainer!.FirstName, a.Trainer.LastName })
                    .Select(g => new
                    {
                        TrainerId = g.Key.TrainerId,
                        TrainerName = g.Key.FirstName + " " + g.Key.LastName,
                        AppointmentCount = g.Count(),
                        Revenue = g.Sum(a => a.Price)
                    })
                    .OrderByDescending(x => x.AppointmentCount)
                    .Take(5)
                    .ToListAsync()
            };

            return Ok(statistics);
        }
    }
}

