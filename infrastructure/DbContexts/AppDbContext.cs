using Application.Interface;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DbContexts;

public partial class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiauditLog> AiauditLogs { get; set; }

    public virtual DbSet<Aiconversation> Aiconversations { get; set; }

    public virtual DbSet<Aireport> Aireports { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<LeaveType> LeaveTypes { get; set; }

    public virtual DbSet<Payroll> Payrolls { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Database=HRM;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiauditLog>(entity =>
        {
            entity.ToTable("AIAuditLog");

            entity.Property(e => e.AiauditLogId).HasColumnName("AIAuditLogId");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ErrorMessage).HasColumnType("text");
            entity.Property(e => e.FeatureName).HasMaxLength(100);
        });

        modelBuilder.Entity<Aiconversation>(entity =>
        {
            entity.ToTable("AIConversation");

            entity.Property(e => e.AiconversationId).HasColumnName("AIConversationId");
            entity.Property(e => e.Airesponse)
                .HasColumnType("text")
                .HasColumnName("AIResponse");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserPrompt).HasColumnType("text");
        });

        modelBuilder.Entity<Aireport>(entity =>
        {
            entity.ToTable("AIReport");

            entity.Property(e => e.AireportId).HasColumnName("AIReportId");
            entity.Property(e => e.GeneratedAt).HasColumnType("datetime");
            entity.Property(e => e.ReportContent).HasColumnType("text");
            entity.Property(e => e.ReportType)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.ToTable("Attendance");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Remarks).HasColumnType("text");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.WorkingHours).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Company");

            entity.Property(e => e.CompanyAddress).HasColumnType("text");
            entity.Property(e => e.CompanyEmail).HasMaxLength(150);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.CompanyPhone).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LogoUrl).HasColumnType("text");
            entity.Property(e => e.SubscriptionPlan).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Department");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DepartmentName).HasMaxLength(100);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EmployeeCode).HasMaxLength(30);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.JoinDate).HasColumnType("datetime");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasColumnType("text");
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Salary).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.ToTable("LeaveRequest");

            entity.Property(e => e.Ainotes)
                .HasColumnType("text")
                .HasColumnName("AINotes");
            entity.Property(e => e.Airecommendation)
                .HasMaxLength(50)
                .HasColumnName("AIRecommendation");
            entity.Property(e => e.ApprovedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FromDate).HasColumnType("datetime");
            entity.Property(e => e.Reason).HasColumnType("text");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.ToDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.ToTable("LeaveType");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.LeaveTypeDescription).HasColumnType("text");
            entity.Property(e => e.LeaveTypeName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Payroll>(entity =>
        {
            entity.ToTable("Payroll");

            entity.Property(e => e.AbsentDeduction).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.BasicSalary).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GeneratedAt).HasColumnType("datetime");
            entity.Property(e => e.LateDeduction).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.NetSalary).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.RefreshTokenId).HasName("PK__RefreshT__F5845E396EBC4379");

            entity.ToTable("RefreshToken");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.RefreshToken1)
                .HasMaxLength(500)
                .HasColumnName("RefreshToken");

            entity.HasOne(d => d.Employee).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK_RefreshToken_Employee");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.RoleName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
