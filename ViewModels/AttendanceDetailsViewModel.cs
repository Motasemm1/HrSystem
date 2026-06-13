using System.ComponentModel.DataAnnotations;
using SmartHrSystem.Models;

namespace SmartHrSystem.ViewModels
{
    public class AttendanceDetailsViewModel
    {
        public int? AttendanceRecordId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string? RoleTitle { get; set; }

        [Required(ErrorMessage = "Attendance date is required.")]
        [DataType(DataType.Date)]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Arrival time is required.")]
        [DataType(DataType.Time)]
        [Display(Name = "Arrival Time")]
        public TimeOnly CheckIn { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Leave Time")]
        public TimeOnly? CheckOut { get; set; }

        [Required(ErrorMessage = "Please choose if the employee worked from office or home.")]
        [Display(Name = "Work Type")]
        public string Status { get; set; } = "Office";

        public string? Notes { get; set; }

        public IEnumerable<AttendanceRecord> AttendanceHistory { get; set; } = new List<AttendanceRecord>();

        public double? TotalHours =>
            CheckOut.HasValue && CheckOut.Value > CheckIn
                ? Math.Round((CheckOut.Value - CheckIn).TotalHours, 2)
                : null;
    }
}
