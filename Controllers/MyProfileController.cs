using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHrSystem.Data;
using SmartHrSystem.Models;
using SmartHrSystem.ViewModels;

namespace SmartHrSystem.Controllers
{
    [Authorize(Roles = "Employee")]
    public class MyProfileController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyProfileController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // helper — gets the linked Employee record for the logged-in user
        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.EmployeeId == null) return null;

            return await _db.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId);
        }

        // GET: /MyProfile
        public async Task<IActionResult> Index()
        {
            var employee = await GetCurrentEmployeeAsync();

            if (employee == null)
            {
                TempData["Error"] = "Your account is not linked to an employee record. Contact HR.";
                return View(new MyProfileViewModel());
            }

            var model = new MyProfileViewModel
            {
                FullName = $"{employee.FirstName} {employee.LastName}",
                Email = employee.Email,
                Phone = employee.Phone,
                Department = employee.Department.Name,
                Role = employee.Role.Title,
                Salary = employee.Salary,
                HireDate = employee.HireDate
            };

            return View(model);
        }

        // GET: /MyProfile/Attendance
        public async Task<IActionResult> Attendance()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return RedirectToAction(nameof(Index));

            var records = await _db.AttendanceRecords
                .Where(a => a.EmployeeId == employee.Id)
                .OrderByDescending(a => a.Date)
                .Select(a => new AttendanceRowDto
                {
                    Date = a.Date,
                    CheckIn = a.CheckIn,
                    CheckOut = a.CheckOut,
                    Status = a.Status,
                    Notes = a.Notes
                })
                .ToListAsync();

            var model = new MyAttendanceViewModel
            {
                Records = records,
                PresentCount = records.Count(r => r.Status == "Present"),
                LateCount = records.Count(r => r.Status == "Late"),
                OnLeaveCount = records.Count(r => r.Status == "On Leave")
            };

            return View(model);
        }

        // GET: /MyProfile/RecordAttendance
        public async Task<IActionResult> RecordAttendance()
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return RedirectToAction(nameof(Index));

            // check if already recorded today
            var today = DateOnly.FromDateTime(DateTime.Today);
            bool alreadyRecorded = await _db.AttendanceRecords
                .AnyAsync(a => a.EmployeeId == employee.Id && a.Date == today);

            if (alreadyRecorded)
            {
                TempData["Error"] = "You have already recorded your attendance for today.";
                return RedirectToAction(nameof(Attendance));
            }

            return View(new RecordAttendanceViewModel());
        }

        // POST: /MyProfile/RecordAttendance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordAttendance(RecordAttendanceViewModel model)
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee == null) return RedirectToAction(nameof(Index));

            var today = DateOnly.FromDateTime(DateTime.Today);

            // double check on POST as well
            bool alreadyRecorded = await _db.AttendanceRecords
                .AnyAsync(a => a.EmployeeId == employee.Id && a.Date == today);

            if (alreadyRecorded)
            {
                TempData["Error"] = "You have already recorded your attendance for today.";
                return RedirectToAction(nameof(Attendance));
            }

            var record = new AttendanceRecord
            {
                EmployeeId = employee.Id,
                Date = today,
                CheckIn = model.CheckIn,
                CheckOut = model.CheckOut,
                Status = model.Status,
                Notes = model.Notes?.Trim()
            };

            _db.AttendanceRecords.Add(record);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Attendance recorded successfully.";
            return RedirectToAction(nameof(Attendance));
        }
    }
}
