using Application.DTOs;
using Application.Interface;
using Domain.Models;
using MediatR;

namespace Application.Features.Auth.Refresh;

public class RefreshCommandHandler(
    IAuthRepository authRepository,
    ITokenService tokenService) : IRequestHandler<RefreshCommand, AuthResponseDto>
{
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly ITokenService _tokenService = tokenService;

    public async Task<AuthResponseDto> Handle(
        RefreshCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate stored token
        var stored = await _authRepository
            .GetRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (stored == null || stored.IsRevoked || stored.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        // 2. Rotate — revoke old, issue new
        await _authRepository.RevokeRefreshTokenAsync(stored, cancellationToken);

        var newAccessToken = _tokenService.GenerateAccessToken(stored.Employee);
        var newRawRefresh = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            RefreshToken1 = newRawRefresh,
            EmployeeId = stored.EmployeeId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _authRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(newAccessToken, newRawRefresh);
    }
}