using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartHrSystem.ViewModels
{
    public class EmployeeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email.")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Hire date is required.")]
        public DateTime HireDate { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Salary is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number.")]
        public decimal Salary { get; set; }

        // ── Portal account fields ──────────────────────────────
        // Required only on Create
        [MaxLength(100)]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string? ConfirmPassword { get; set; }

        // Optional on Edit — only filled if HR wants to reset password
        public string? NewPassword { get; set; }

        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string? ConfirmNewPassword { get; set; }

        // dropdowns
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Departments { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Roles { get; set; } = new();

    }
}