using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHrSystem.Data;
using SmartHrSystem.Models;
using SmartHrSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartHrSystem.Controllers
{
    [Authorize(Roles = "HR")]
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;


        public EmployeesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _db = context;
            _userManager = userManager;
        }

        // 1. GET: Employees 
        public async Task<IActionResult> Index()
        {
            var employees = await _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .ToListAsync();
            return View(employees);
        }

        // 2. GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new EmployeeFormViewModel
            {
                Departments = await _db.Departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync(),
                Roles = await _db.Roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Title }).ToListAsync()
            };
            return View(viewModel);
        }

        // POST: /Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeFormViewModel model)
        {
            // password required on create
            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(model.Password), "Password is required.");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // check duplicate employee email
            bool emailExists = await _db.Employees
                .AnyAsync(e => e.Email.ToLower() == model.Email.ToLower());

            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "An employee with this email already exists.");
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // check if email already used in Identity
            bool identityEmailExists = await _userManager.FindByEmailAsync(model.Email) != null;
            if (identityEmailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already linked to a system account.");
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // 1. create Employee record first
            var employee = new Employee
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Email = model.Email.Trim(),
                Phone = model.Phone?.Trim(),
                HireDate = model.HireDate,
                DepartmentId = model.DepartmentId,
                RoleId = model.RoleId,
                Salary = model.Salary,
                CreatedAt = DateTime.UtcNow
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync(); // save to get the Id

            // 2. create Identity account linked to the employee
            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                EmailConfirmed = true,
                EmployeeId = employee.Id
            };

            var result = await _userManager.CreateAsync(user, model.Password!);

            if (!result.Succeeded)
            {
                // identity failed — rollback the employee record
                _db.Employees.Remove(employee);
                await _db.SaveChangesAsync();

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                await PopulateDropdownsAsync(model);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Employee");

            TempData["Success"] = $"{employee.FirstName} {employee.LastName} added successfully with a portal account.";
            return RedirectToAction(nameof(Index));
        }


        // GET: /Employees/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var model = new EmployeeFormViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Phone = employee.Phone,
                HireDate = employee.HireDate,
                DepartmentId = employee.DepartmentId,
                RoleId = employee.RoleId,
                Salary = employee.Salary
            };

            await PopulateDropdownsAsync(model);
            return View(model);
        }

        // POST: /Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeFormViewModel model)
        {
            // remove password validation on edit — it's optional
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var employee = await _db.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            // check duplicate email excluding current employee
            bool emailExists = await _db.Employees
                .AnyAsync(e => e.Email.ToLower() == model.Email.ToLower() && e.Id != id);

            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already used by another employee.");
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // update employee record
            employee.FirstName = model.FirstName.Trim();
            employee.LastName = model.LastName.Trim();
            employee.Email = model.Email.Trim();
            employee.Phone = model.Phone?.Trim();
            employee.HireDate = model.HireDate;
            employee.DepartmentId = model.DepartmentId;
            employee.RoleId = model.RoleId;
            employee.Salary = model.Salary;

            await _db.SaveChangesAsync();

            // update Identity account email if it changed
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.EmployeeId == id);

            if (user != null)
            {
                user.Email = model.Email.Trim();
                user.UserName = model.Email.Trim();
                await _userManager.UpdateAsync(user);

                // reset password if HR provided a new one
                if (!string.IsNullOrWhiteSpace(model.NewPassword))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);

                        await PopulateDropdownsAsync(model);
                        return View(model);
                    }
                }
            }

            TempData["Success"] = $"{employee.FirstName} {employee.LastName} updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Employees/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _db.Employees
                .Include(e => e.AttendanceRecords)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            // delete linked Identity account first
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.EmployeeId == id);

            if (user != null)
                await _userManager.DeleteAsync(user);

            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"{employee.FirstName} {employee.LastName} deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        private async Task PopulateDropdownsAsync(EmployeeFormViewModel model)
        {
            model.Departments = await _db.Departments
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
                .ToListAsync();

            model.Roles = await _db.Roles
                .OrderBy(r => r.Title)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Title
                })
                .ToListAsync();
        }
    }
}