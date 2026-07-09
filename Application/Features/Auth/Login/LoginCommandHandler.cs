using Application.DTOs;
using Application.Interface;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Auth.Login;

public class LoginCommandHandler(
    IAuthRepository authRepository,
    ITokenService tokenService) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<AuthResponseDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Find employee
        var employee = await _authRepository
                           .GetEmployeeByEmailAsync(request.Email, cancellationToken) ??
                       throw new UnauthorizedAccessException("Invalid credentials.");

        // 2. Verify password
        var verify = new PasswordHasher<Domain.Models.Employee>().VerifyHashedPassword(employee, employee.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // 3. Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(employee);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();

        // 4. Persist refresh token
        var refreshToken = new RefreshToken
        {
            RefreshToken1 = rawRefreshToken,
            EmployeeId = employee.EmployeeId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _authRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(accessToken, rawRefreshToken);
    }
}