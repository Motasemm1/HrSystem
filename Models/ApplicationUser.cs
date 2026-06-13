using Microsoft.AspNetCore.Identity;

namespace SmartHrSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // null = HR user, has value = Employee user
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
    }
}
