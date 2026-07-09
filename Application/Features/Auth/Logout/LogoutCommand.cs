using MediatR;

namespace Application.Features.Auth.Logout
{
    public record LogoutCommand(string RefreshToken) : IRequest<Unit>;
}
