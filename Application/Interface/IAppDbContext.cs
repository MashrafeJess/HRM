using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Interface;

public interface IAppDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Company> Companies { get; }
    DbSet<Department> Departments { get; }
    DbSet<Role> Roles { get; }
    DbSet<Attendance> Attendances { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}