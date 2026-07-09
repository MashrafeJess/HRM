using Application.Interface;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AuthRepository(IAppDbContext context) : IAuthRepository
{
    private readonly IAppDbContext _context = context;

    public async Task<Employee?> GetEmployeeByEmailAsync(
        string email, CancellationToken ct)
        => await _context.Employees
            .Include(r => r.Role)
            .FirstOrDefaultAsync(e => e.Email == email, ct);

    public async Task<RefreshToken?> GetRefreshTokenAsync(
        string token, CancellationToken ct)
        => await _context.RefreshTokens
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.RefreshToken1 == token, ct);

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct)
        => await _context.RefreshTokens.AddAsync(token, ct);

    public async Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct)
    {
        token.IsRevoked = true;
        _context.RefreshTokens.Update(token);
    }
    
    public async Task<Employee?> GetEmployeeByIdAsync(long? employeeId, CancellationToken ct)
        => await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId, ct);

    public async Task UpdateEmployeeAsync(Employee employee, CancellationToken ct)
        => _context.Employees.Update(employee);

    public async Task RevokeAllUserTokensAsync(long employeeId, CancellationToken ct)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.EmployeeId == employeeId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.IsRevoked = true;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
        => await _context.SaveChangesAsync(ct);
}