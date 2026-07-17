using Application.DTOs;
using MediatR;

namespace Application.Features.Role.Upsert;

public record EditRoleCommand(RoleDto Dto) : IRequest<RoleDto>;
