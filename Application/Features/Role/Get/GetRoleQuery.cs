using Application.DTOs;
using MediatR;

namespace Application.Features.Role.Get;

public record GetRoleQuery : IRequest<List<RoleDto>>;