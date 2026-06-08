using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHrSystem.Data;
using SmartHrSystem.ViewModels;

namespace SmartHrSystem.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly AppDbContext _db;

        public RolesController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Roles
        public async Task<IActionResult> Index()
        {
            var roles = await _db.Roles
                .Include(r => r.Employees)
                .OrderBy(r => r.Title)
                .ToListAsync();

            return View(roles);
        }

        // GET: /Roles/Create
        public IActionResult Create()
        {
            return View(new RoleViewModel());
        }

        // POST: /Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool titleExists = await _db.Roles
                .AnyAsync(r => r.Title.ToLower() == model.Title.ToLower());

            if (titleExists)
            {
                ModelState.AddModelError(nameof(model.Title), "A role with this title already exists.");
                return View(model);
            }

            var role = new Models.Role
            {
                Title = model.Title.Trim(),
                Description = model.Description?.Trim()
            };

            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Role '{role.Title}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Roles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _db.Roles.FindAsync(id);

            if (role == null)
                return NotFound();

            var model = new RoleViewModel
            {
                Id = role.Id,
                Title = role.Title,
                Description = role.Description
            };

            return View(model);
        }

        // POST: /Roles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var role = await _db.Roles.FindAsync(id);

            if (role == null)
                return NotFound();

            bool titleExists = await _db.Roles
                .AnyAsync(r => r.Title.ToLower() == model.Title.ToLower() && r.Id != id);

            if (titleExists)
            {
                ModelState.AddModelError(nameof(model.Title), "A role with this title already exists.");
                return View(model);
            }

            role.Title = model.Title.Trim();
            role.Description = model.Description?.Trim();

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Role '{role.Title}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Roles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _db.Roles
                .Include(r => r.Employees)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                return NotFound();

            if (role.Employees.Any())
            {
                TempData["Error"] = $"Cannot delete '{role.Title}' because it has employees assigned.";
                return RedirectToAction(nameof(Index));
            }

            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Role '{role.Title}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
