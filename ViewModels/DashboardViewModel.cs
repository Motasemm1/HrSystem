namespace SmartHrSystem.ViewModels
{
    public class DashboardViewModel
    {
        // stat cards
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int PresentToday { get; set; }
        public int AbsentToday { get; set; }
        public decimal TotalMonthlySalary { get; set; }

        // recent employees table
        public List<RecentEmployeeDto> RecentEmployees { get; set; } = new();

        // departments summary table
        public List<DepartmentSummaryDto> DepartmentSummaries { get; set; } = new();

    }

    public class RecentEmployeeDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DepartmentSummaryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int EmployeeCount { get; set; }
    }
}
