using FitnessCenter.Data;
using FitnessCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TrainersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TrainersApi
        // Tüm antrenörleri listele
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainers()
        {
            var trainers = await _context.Trainers
                .Where(t => t.IsActive)
                .Include(t => t.Gym)
                .Include(t => t.TrainerServices!)
                    .ThenInclude(ts => ts.Service)
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    t.Email,
                    t.PhoneNumber,
                    t.Specializations,
                    t.ExperienceYears,
                    t.ProfileImageUrl,
                    GymName = t.Gym != null ? t.Gym.Name : null,
                    Services = t.TrainerServices!.Select(ts => new
                    {
                        ts.Service!.Id,
                        ts.Service.Name,
                        ts.Service.Price,
                        ts.Service.DurationMinutes
                    })
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // GET: api/TrainersApi/5
        // Belirli bir antrenörü getir
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTrainer(int id)
        {
            var trainer = await _context.Trainers
                .Where(t => t.Id == id)
                .Include(t => t.Gym)
                .Include(t => t.TrainerServices!)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.Availabilities)
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    t.Email,
                    t.PhoneNumber,
                    t.Specializations,
                    t.Biography,
                    t.ExperienceYears,
                    t.ProfileImageUrl,
                    t.IsActive,
                    GymName = t.Gym != null ? t.Gym.Name : null,
                    Services = t.TrainerServices!.Select(ts => new
                    {
                        ts.Service!.Id,
                        ts.Service.Name,
                        ts.Service.Price
                    }),
                    Availabilities = t.Availabilities!.Where(a => a.IsActive).Select(a => new
                    {
                        a.DayOfWeek,
                        StartTime = a.StartTime.ToString(@"hh\:mm"),
                        EndTime = a.EndTime.ToString(@"hh\:mm")
                    })
                })
                .FirstOrDefaultAsync();

            if (trainer == null)
            {
                return NotFound(new { message = "Antrenör bulunamadı" });
            }

            return Ok(trainer);
        }

        // GET: api/TrainersApi/Available?date=2024-01-15
        // Belirli bir tarihte müsait antrenörleri getir
        [HttpGet("Available")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableTrainers([FromQuery] DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            // Müsait antrenörleri bul (LINQ sorgusu)
            var availableTrainers = await _context.Trainers
                .Where(t => t.IsActive)
                .Where(t => t.Availabilities!.Any(a => a.DayOfWeek == dayOfWeek && a.IsActive))
                .Include(t => t.TrainerServices!)
                    .ThenInclude(ts => ts.Service)
                .Include(t => t.Availabilities)
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    t.Specializations,
                    t.ExperienceYears,
                    Services = t.TrainerServices!.Select(ts => new
                    {
                        ts.Service!.Id,
                        ts.Service.Name,
                        ts.Service.Price
                    }),
                    AvailableHours = t.Availabilities!
                        .Where(a => a.DayOfWeek == dayOfWeek && a.IsActive)
                        .Select(a => new
                        {
                            StartTime = a.StartTime.ToString(@"hh\:mm"),
                            EndTime = a.EndTime.ToString(@"hh\:mm")
                        })
                })
                .ToListAsync();

            return Ok(new
            {
                Date = date.ToString("yyyy-MM-dd"),
                DayOfWeek = dayOfWeek.ToString(),
                AvailableTrainers = availableTrainers,
                TotalCount = availableTrainers.Count
            });
        }

        // GET: api/TrainersApi/BySpecialization?specialization=yoga
        // Uzmanlık alanına göre antrenörleri filtrele
        [HttpGet("BySpecialization")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainersBySpecialization([FromQuery] string specialization)
        {
            if (string.IsNullOrWhiteSpace(specialization))
            {
                return BadRequest(new { message = "Uzmanlık alanı belirtilmelidir" });
            }

            var trainers = await _context.Trainers
                .Where(t => t.IsActive)
                .Where(t => t.Specializations != null && 
                           t.Specializations.ToLower().Contains(specialization.ToLower()))
                .Select(t => new
                {
                    t.Id,
                    t.FirstName,
                    t.LastName,
                    FullName = t.FirstName + " " + t.LastName,
                    t.Specializations,
                    t.ExperienceYears,
                    t.Biography
                })
                .ToListAsync();

            return Ok(new
            {
                Specialization = specialization,
                Trainers = trainers,
                TotalCount = trainers.Count
            });
        }
    }
}


