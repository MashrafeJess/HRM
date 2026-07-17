using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Role.Upsert;

public class EditRoleCommandHandler(IRoleRepository roleRepository)
    : IRequestHandler<EditRoleCommand, RoleDto>
{
    private readonly IRoleRepository _roleRepository = roleRepository;

    public async Task<RoleDto> Handle(EditRoleCommand request, CancellationToken cancellationToken)
    {
        Domain.Models.Role role;

        if (request.Dto.RoleId is null or 0)
        {
            role = new Domain.Models.Role
            {
                RoleName = request.Dto.RoleName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }
        else
        {
            role = await _roleRepository.GetRoleById(request.Dto.RoleId.Value, cancellationToken);

            role.RoleName = request.Dto.RoleName;
            role.UpdatedAt = DateTime.UtcNow;
            role.IsActive = request.Dto.IsActive;
        }

        role = await _roleRepository.EditRole(role, cancellationToken);

        return new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            IsActive = role.IsActive
        };
    }
}
