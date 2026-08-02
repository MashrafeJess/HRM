using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Employee = Domain.Models.Employee;
using Role = Domain.Models.Role;

namespace Application.Interface;

public interface IAppDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Company> Companies { get; }
    DbSet<Department> Departments { get; }
    DbSet<Role> Roles { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<Payroll> Payrolls { get; }
    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}