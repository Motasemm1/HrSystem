using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartHrSystem.Data;
using SmartHrSystem.Models;
using SmartHrSystem.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace SmartHrSystem.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. GET: Employees 
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
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
                Departments = await _context.Departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync(),
                Roles = await _context.Roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Title }).ToListAsync()
            };
            return View(viewModel);
        }

        // 3. POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeFormViewModel model)
        {
            
            if (model.Salary <= 0)
            {
                ModelState.AddModelError("Salary", "Salary must be a positive number greater than 0.");
            }

            
            var emailExists = await _context.Employees.AnyAsync(e => e.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email address is already registered to another employee.");
            }

           
            var phoneExists = await _context.Employees.AnyAsync(e => e.Phone == model.Phone);
            if (phoneExists)
            {
                ModelState.AddModelError("Phone", "This phone number is already registered to another employee.");
            }

           
            if (ModelState.IsValid)
            {
                var employee = new Employee
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    HireDate = model.HireDate,
                    Salary = model.Salary,
                    DepartmentId = model.DepartmentId,
                    RoleId = model.RoleId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            
            model.Departments = await _context.Departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync();
            model.Roles = await _context.Roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Title }).ToListAsync();
            return View(model);
        }

        // 4. GET: Employees/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var viewModel = new EmployeeFormViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Phone = employee.Phone ?? string.Empty,
                HireDate = employee.HireDate,
                Salary = employee.Salary,
                DepartmentId = employee.DepartmentId,
                RoleId = employee.RoleId,
                Departments = await _context.Departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync(),
                Roles = await _context.Roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Title }).ToListAsync()
            };

            return View(viewModel);
        }

        // 5. POST: Employees/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            
            if (model.Salary <= 0)
            {
                ModelState.AddModelError("Salary", "Salary must be a positive number greater than 0.");
            }

           
            var emailExists = await _context.Employees.AnyAsync(e => e.Email == model.Email && e.Id != id);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email address is already registered to another employee.");
            }

            
            var phoneExists = await _context.Employees.AnyAsync(e => e.Phone == model.Phone && e.Id != id);
            if (phoneExists)
            {
                ModelState.AddModelError("Phone", "This phone number is already registered to another employee.");
            }

            if (ModelState.IsValid)
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null) return NotFound();

                employee.FirstName = model.FirstName;
                employee.LastName = model.LastName;
                employee.Email = model.Email;
                employee.Phone = model.Phone;
                employee.HireDate = model.HireDate;
                employee.Salary = model.Salary;
                employee.DepartmentId = model.DepartmentId;
                employee.RoleId = model.RoleId;

                _context.Update(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

           
            model.Departments = await _context.Departments.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync();
            model.Roles = await _context.Roles.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Title }).ToListAsync();
            return View(model);
        }

        // 6. POST: Employees/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}