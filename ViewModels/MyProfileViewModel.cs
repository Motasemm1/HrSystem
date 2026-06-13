namespace SmartHrSystem.ViewModels;

public class MyProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
}

public class MyAttendanceViewModel
{
    public List<AttendanceRowDto> Records { get; set; } = new();
    public int PresentCount { get; set; }
    public int LateCount { get; set; }
    public int OnLeaveCount { get; set; }
}

public class AttendanceRowDto
{
    public DateOnly Date { get; set; }
    public TimeOnly CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class RecordAttendanceViewModel
{
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public TimeOnly CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    public string Status { get; set; } = "Present";
    public string? Notes { get; set; }
}