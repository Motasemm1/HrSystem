using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartHrSystem.ViewModels
{
    public class EmployeeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [Display(Name = "الاسم الأول")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "الاسم الأخير مطلوب")]
        [Display(Name = "الاسم الأخير")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "تاريخ التعيين مطلوب")]
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ التعيين")]
        public DateTime HireDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "الراتب مطلوب")]
        [Display(Name = "الراتب")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار القسم")]
        [Display(Name = "القسم")]
        public int DepartmentId { get; set; }

        public IEnumerable<SelectListItem>? Departments { get; set; }

        [Required(ErrorMessage = "الرجاء اختيار الصلاحية")]
        [Display(Name = "الصلاحية (Role)")]
        public int RoleId { get; set; }

        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}