using FitnessCenter.Data;
using FitnessCenter.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenter.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MembersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Members
        public async Task<IActionResult> Index()
        {
            var members = await _userManager.GetUsersInRoleAsync("Member");
            return View(members.OrderBy(m => m.FirstName).ThenBy(m => m.LastName).ToList());
        }

        // GET: Admin/Members/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _userManager.FindByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            // Üyenin randevularını al
            ViewBag.Appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.UserId == id)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(10)
                .ToListAsync();

            return View(member);
        }

        // POST: Admin/Members/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var member = await _userManager.FindByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            member.IsActive = !member.IsActive;
            await _userManager.UpdateAsync(member);

            TempData["SuccessMessage"] = member.IsActive ? "Üye aktif edildi." : "Üye pasif edildi.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Members/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _userManager.FindByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Admin/Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var member = await _userManager.FindByIdAsync(id);
            if (member != null)
            {
                // Üyeyi silmek yerine pasif yap
                member.IsActive = false;
                await _userManager.UpdateAsync(member);
                TempData["SuccessMessage"] = "Üye pasif edildi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


