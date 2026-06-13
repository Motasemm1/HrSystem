namespace SmartHrSystem.ViewModels
{
    public class AttendanceIndexViewModel
    {
        public string? SearchTerm { get; set; }
        public IEnumerable<AttendanceEmployeeListItemViewModel> Employees { get; set; } = new List<AttendanceEmployeeListItemViewModel>();
    }

    public class AttendanceEmployeeListItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? DepartmentName { get; set; }
        public string? RoleTitle { get; set; }
    }
}
