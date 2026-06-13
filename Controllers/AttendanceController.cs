using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHrSystem.Data;
using SmartHrSystem.Models;
using SmartHrSystem.ViewModels;

namespace SmartHrSystem.Controllers
{
    [Authorize(Roles = "HR")]
    public class AttendanceController : Controller
    {
        private static readonly string[] AllowedStatuses = { "Office", "Home" };
        private readonly AppDbContext _context;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm)
        {
            var employeesQuery = _context.Employees.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearch = searchTerm.Trim();
                employeesQuery = employeesQuery.Where(e =>
                    e.FirstName.Contains(normalizedSearch) ||
                    e.LastName.Contains(normalizedSearch) ||
                    (e.FirstName + " " + e.LastName).Contains(normalizedSearch));
            }

            var viewModel = new AttendanceIndexViewModel
            {
                SearchTerm = searchTerm,
                Employees = await employeesQuery
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .Select(e => new AttendanceEmployeeListItemViewModel
                    {
                        Id = e.Id,
                        FullName = e.FirstName + " " + e.LastName,
                        Email = e.Email,
                        Phone = e.Phone,
                        DepartmentName = e.Department.Name,
                        RoleTitle = e.Role.Title
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> AttendanceDetails(int id, DateOnly? date)
        {
            var employee = await GetAttendanceEmployeeAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            var attendanceDate = date ?? DateOnly.FromDateTime(DateTime.Today);
            var existingRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.EmployeeId == id && a.Date == attendanceDate);

            return View(await BuildDetailsViewModelAsync(employee, attendanceDate, existingRecord));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttendanceDetails(AttendanceDetailsViewModel model)
        {
            if (!AllowedStatuses.Contains(model.Status))
            {
                ModelState.AddModelError(nameof(model.Status), "Please choose Office or Home.");
            }

            if (model.CheckOut.HasValue && model.CheckOut.Value <= model.CheckIn)
            {
                ModelState.AddModelError(nameof(model.CheckOut), "Leave time must be after arrival time.");
            }

            var employee = await GetAttendanceEmployeeAsync(model.EmployeeId);

            if (employee == null)
            {
                return NotFound();
            }

            var existingRecord = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.EmployeeId == model.EmployeeId && a.Date == model.Date);

            if (!ModelState.IsValid)
            {
                return View(await BuildDetailsViewModelAsync(employee, model.Date, existingRecord, model));
            }

            if (existingRecord == null)
            {
                existingRecord = new AttendanceRecord
                {
                    EmployeeId = model.EmployeeId,
                    Date = model.Date
                };
                _context.AttendanceRecords.Add(existingRecord);
            }

            existingRecord.CheckIn = model.CheckIn;
            existingRecord.CheckOut = model.CheckOut;
            existingRecord.Status = model.Status;
            existingRecord.Notes = model.Notes;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Attendance record saved successfully.";

            return RedirectToAction(nameof(AttendanceDetails), new { id = model.EmployeeId, date = model.Date.ToString("yyyy-MM-dd") });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttendanceRecord(int recordId, int employeeId)
        {
            var record = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.Id == recordId && a.EmployeeId == employeeId);

            if (record == null)
            {
                return NotFound();
            }

            _context.AttendanceRecords.Remove(record);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Attendance record deleted successfully.";

            return RedirectToAction(nameof(AttendanceDetails), new { id = employeeId });
        }
        private async Task<AttendanceDetailsViewModel> BuildDetailsViewModelAsync(
            AttendanceEmployeeListItemViewModel employee,
            DateOnly attendanceDate,
            AttendanceRecord? existingRecord,
            AttendanceDetailsViewModel? postedModel = null)
        {
            var history = await _context.AttendanceRecords
                .Where(a => a.EmployeeId == employee.Id)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .ToListAsync();

            return new AttendanceDetailsViewModel
            {
                AttendanceRecordId = existingRecord?.Id ?? postedModel?.AttendanceRecordId,
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                DepartmentName = employee.DepartmentName,
                RoleTitle = employee.RoleTitle,
                Date = postedModel?.Date ?? existingRecord?.Date ?? attendanceDate,
                CheckIn = postedModel?.CheckIn ?? existingRecord?.CheckIn ?? new TimeOnly(9, 0),
                CheckOut = postedModel?.CheckOut ?? existingRecord?.CheckOut,
                Status = postedModel?.Status ?? existingRecord?.Status ?? "Office",
                Notes = postedModel?.Notes ?? existingRecord?.Notes,
                AttendanceHistory = history
            };
        }

        private Task<AttendanceEmployeeListItemViewModel?> GetAttendanceEmployeeAsync(int employeeId)
        {
            return _context.Employees
                .Where(e => e.Id == employeeId)
                .Select(e => new AttendanceEmployeeListItemViewModel
                {
                    Id = e.Id,
                    FullName = e.FirstName + " " + e.LastName,
                    Email = e.Email,
                    Phone = e.Phone,
                    DepartmentName = e.Department.Name,
                    RoleTitle = e.Role.Title
                })
                .FirstOrDefaultAsync();
        }
    }
}
