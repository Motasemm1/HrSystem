namespace SmartHrSystem.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly CheckIn { get; set; }
        public TimeOnly? CheckOut { get; set; }
        public string Status { get; set; } = string.Empty; // Present, Late, On Leave
        public string? Notes { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
    }
}
