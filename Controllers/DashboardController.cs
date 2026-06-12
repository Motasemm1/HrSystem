using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHrSystem.Data;
using SmartHrSystem.ViewModels;

namespace SmartHrSystem.Controllers
{

    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var totalEmployees = await _db.Employees.CountAsync();
            var totalDepartments = await _db.Departments.CountAsync();
            var presentToday = await _db.AttendanceRecords
                                        .CountAsync(a => a.Date == today && a.Status == "Present");
            var absentToday = totalEmployees - await _db.AttendanceRecords
                                        .CountAsync(a => a.Date == today);
            var totalMonthlySalary = await _db.Employees.SumAsync(e => e.Salary);

            var recentEmployees = await _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .OrderByDescending(e => e.CreatedAt)
                .Take(5)
                .Select(e => new RecentEmployeeDto
                {
                    FullName = e.FirstName + " " + e.LastName,
                    Department = e.Department.Name,
                    Role = e.Role.Title,
                    HireDate = e.HireDate,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            var departmentSummaries = await _db.Departments
                .Include(d => d.Employees)
                .OrderByDescending(d => d.Employees.Count)
                .Select(d => new DepartmentSummaryDto
                {
                    Name = d.Name,
                    Description = d.Description,
                    EmployeeCount = d.Employees.Count
                })
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalEmployees = totalEmployees,
                TotalDepartments = totalDepartments,
                PresentToday = presentToday,
                AbsentToday = absentToday < 0 ? 0 : absentToday,
                TotalMonthlySalary = totalMonthlySalary,
                RecentEmployees = recentEmployees,
                DepartmentSummaries = departmentSummaries
            };

            return View(model);
        }
    }
}
