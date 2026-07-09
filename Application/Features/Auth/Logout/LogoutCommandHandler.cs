using Application.Interface;
using MediatR;

namespace Application.Features.Auth.Logout;

public class LogoutCommandHandler(IAuthRepository authRepository) : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var stored = await authRepository
            .GetRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (stored is not { IsRevoked: false }) return Unit.Value;
        await authRepository.RevokeRefreshTokenAsync(stored, cancellationToken);
        await authRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}