using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartHrSystem.Models;
using System.Collections.Generic;

namespace SmartHrSystem.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Models.Role> Roles { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Employee → Department (restrict delete if employees exist)
            builder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee → Role (restrict delete if employees exist)
            builder.Entity<Employee>()
                .HasOne(e => e.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // AttendanceRecord → Employee (cascade delete)
            builder.Entity<AttendanceRecord>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique email per employee
            builder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            // Unique department name
            builder.Entity<Department>()
                .HasIndex(d => d.Name)
                .IsUnique();

            // Unique role title
            builder.Entity<Models.Role>()
                .HasIndex(r => r.Title)
                .IsUnique();

            // One attendance record per employee per day
            builder.Entity<AttendanceRecord>()
                .HasIndex(a => new { a.EmployeeId, a.Date })
                .IsUnique();

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Employee)
                .WithOne()
                .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }
}
