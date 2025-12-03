using FitnessCenter.Data;
using FitnessCenter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ServicesApi
        // Tüm hizmetleri listele
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetServices([FromQuery] ServiceCategory? category)
        {
            var query = _context.Services
                .Where(s => s.IsActive)
                .Include(s => s.Gym)
                .AsQueryable();

            // LINQ ile kategori filtreleme
            if (category.HasValue)
            {
                query = query.Where(s => s.Category == category.Value);
            }

            var services = await query
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.DurationMinutes,
                    s.Price,
                    Category = s.Category.ToString(),
                    s.ImageUrl,
                    GymName = s.Gym != null ? s.Gym.Name : null
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = services.Count,
                Services = services
            });
        }

        // GET: api/ServicesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetService(int id)
        {
            var service = await _context.Services
                .Where(s => s.Id == id)
                .Include(s => s.Gym)
                .Include(s => s.TrainerServices!)
                    .ThenInclude(ts => ts.Trainer)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.DurationMinutes,
                    s.Price,
                    Category = s.Category.ToString(),
                    s.ImageUrl,
                    s.IsActive,
                    GymName = s.Gym != null ? s.Gym.Name : null,
                    Trainers = s.TrainerServices!.Where(ts => ts.Trainer!.IsActive).Select(ts => new
                    {
                        ts.Trainer!.Id,
                        ts.Trainer.FirstName,
                        ts.Trainer.LastName,
                        FullName = ts.Trainer.FirstName + " " + ts.Trainer.LastName
                    })
                })
                .FirstOrDefaultAsync();

            if (service == null)
            {
                return NotFound(new { message = "Hizmet bulunamadı" });
            }

            return Ok(service);
        }

        // GET: api/ServicesApi/Categories
        // Tüm kategorileri ve sayılarını getir
        [HttpGet("Categories")]
        public async Task<ActionResult<IEnumerable<object>>> GetCategories()
        {
            var categories = await _context.Services
                .Where(s => s.IsActive)
                .GroupBy(s => s.Category)
                .Select(g => new
                {
                    Category = g.Key.ToString(),
                    Count = g.Count(),
                    AveragePrice = g.Average(s => s.Price),
                    MinPrice = g.Min(s => s.Price),
                    MaxPrice = g.Max(s => s.Price)
                })
                .OrderBy(x => x.Category)
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/ServicesApi/PriceRange?min=100&max=500
        // Fiyat aralığına göre filtrele
        [HttpGet("PriceRange")]
        public async Task<ActionResult<IEnumerable<object>>> GetServicesByPriceRange(
            [FromQuery] decimal min = 0,
            [FromQuery] decimal max = decimal.MaxValue)
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .Where(s => s.Price >= min && s.Price <= max)
                .OrderBy(s => s.Price)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.DurationMinutes,
                    s.Price,
                    Category = s.Category.ToString()
                })
                .ToListAsync();

            return Ok(new
            {
                MinPrice = min,
                MaxPrice = max,
                TotalCount = services.Count,
                Services = services
            });
        }
    }
}

