using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Employee_Task_and_Attendance_Management_System.Models;

public partial class EmployeeTaskAttendanceDbContext : DbContext
{
    public EmployeeTaskAttendanceDbContext()
    {
    }

    public EmployeeTaskAttendanceDbContext(DbContextOptions<EmployeeTaskAttendanceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Leaf> Leaves { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-J79V5T9B\\SQLEXPRESS;Database=EmployeeTaskAttendanceDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC07683FCB54");

            entity.ToTable("Attendance");

            entity.HasIndex(e => new { e.Date, e.Status }, "IX_Attendance_Date_Status");

            entity.HasIndex(e => new { e.EmployeeId, e.Date }, "UX_Attendance_Employee_Date").IsUnique();

            entity.Property(e => e.CheckIn).HasPrecision(0);
            entity.Property(e => e.CheckOut).HasPrecision(0);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.WorkingHours).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_Users");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC07F7106A78");

            entity.HasIndex(e => e.Name, "UX_Departments_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Leaf>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Leaves__3214EC07BB077A23");

            entity.HasIndex(e => new { e.EmployeeId, e.StartDate }, "IX_Leaves_EmployeeId_StartDate");

            entity.HasIndex(e => e.Status, "IX_Leaves_Status");

            entity.Property(e => e.LeaveType).HasMaxLength(30);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Employee).WithMany(p => p.Leaves)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Leaves_Users");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tasks__3214EC072ACF147A");

            entity.HasIndex(e => e.AssignedBy, "IX_Tasks_AssignedBy");

            entity.HasIndex(e => new { e.AssignedTo, e.Status }, "IX_Tasks_AssignedTo_Status");

            entity.HasIndex(e => e.Deadline, "IX_Tasks_Deadline");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())", "DF_Tasks_CreatedAt");
            entity.Property(e => e.Deadline).HasPrecision(0);
            entity.Property(e => e.Priority).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.AssignedByNavigation).WithMany(p => p.TaskAssignedByNavigations)
                .HasForeignKey(d => d.AssignedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_AssignedBy_Users");

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.TaskAssignedToNavigations)
                .HasForeignKey(d => d.AssignedTo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_AssignedTo_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0776D99DF9");

            entity.HasIndex(e => e.DepartmentId, "IX_Users_DepartmentId");

            entity.HasIndex(e => e.Role, "IX_Users_Role");

            entity.HasIndex(e => e.Email, "UX_Users_Email").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Role).HasMaxLength(20);

            entity.HasOne(d => d.Department).WithMany(p => p.Users)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Users_Departments");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
