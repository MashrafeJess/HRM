using Application.DTOs;
using MediatR;

namespace Application.Features.Role.GetById;

public record GetRoleByIdQuery(long Id) : IRequest<RoleDto>;