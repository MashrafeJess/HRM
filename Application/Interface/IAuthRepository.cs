using Domain.Models;

namespace Application.Interface
{
    public interface IAuthRepository
    {
        Task<Employee?> GetEmployeeByEmailAsync(string email, CancellationToken ct);
        Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct);
        Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct);
        Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
        Task<Employee?> GetEmployeeByIdAsync(long? employeeId, CancellationToken ct);
        Task UpdateEmployeeAsync(Employee employee, CancellationToken ct);
        Task RevokeAllUserTokensAsync(long employeeId, CancellationToken ct);
    }
}
