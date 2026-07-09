using Domain.Models;

namespace Application.Interface
{
    public interface ITokenService
    {
        string GenerateAccessToken(Employee employee);
        string GenerateRefreshToken();
    }
}
