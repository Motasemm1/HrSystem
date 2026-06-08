using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHrSystem.Data;
using SmartHrSystem.Models;
using SmartHrSystem.ViewModels;

namespace SmartHrSystem.Controllers
{
    [Authorize]
    public class DepartmentsController : Controller
    {
        private readonly AppDbContext _db;

        public DepartmentsController(AppDbContext db)
        {
            _db = db;
        }
        // GET: /Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _db.Departments
                .Include(d => d.Employees)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return View(departments);
        }

        // GET: /Departments/Create
        public IActionResult Create()
        {
            return View(new DepartmentViewModel());
        }

        // POST: /Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // check for duplicate name
            bool nameExists = await _db.Departments
                .AnyAsync(d => d.Name.ToLower() == model.Name.ToLower());

            if (nameExists)
            {
                ModelState.AddModelError(nameof(model.Name), "A department with this name already exists.");
                return View(model);
            }

            var department = new Department
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim()
            };

            _db.Departments.Add(department);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Department '{department.Name}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Departments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var department = await _db.Departments.FindAsync(id);

            if (department == null)
                return NotFound();

            var model = new DepartmentViewModel
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description
            };

            return View(model);
        }

        // POST: /Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var department = await _db.Departments.FindAsync(id);

            if (department == null)
                return NotFound();

            // check duplicate name but exclude current record
            bool nameExists = await _db.Departments
                .AnyAsync(d => d.Name.ToLower() == model.Name.ToLower() && d.Id != id);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(model.Name), "A department with this name already exists.");
                return View(model);
            }

            department.Name = model.Name.Trim();
            department.Description = model.Description?.Trim();

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Department '{department.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        // POST: /Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _db.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            if (department.Employees.Any())
            {
                TempData["Error"] = $"Cannot delete '{department.Name}' because it has employees assigned.";
                return RedirectToAction(nameof(Index));
            }

            _db.Departments.Remove(department);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Department '{department.Name}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
