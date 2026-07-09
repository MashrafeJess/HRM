using Application.DTOs;
using MediatR;

namespace Application.Features.Auth.Refresh
{
    public record RefreshCommand(string RefreshToken) : IRequest<AuthResponseDto>;

}
