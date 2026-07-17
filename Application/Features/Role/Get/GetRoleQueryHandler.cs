using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Role.Get;

public class GetRoleQueryHandler(IRoleRepository repository) : IRequestHandler<GetRoleQuery, List<RoleDto>>
{
    private readonly IRoleRepository _repository = repository;
    public async Task<List<RoleDto>> Handle(GetRoleQuery request, CancellationToken cancellationToken)
    {
        var roles = await _repository.GetAllRoles();
        return roles.Select(r => new RoleDto
        {
            RoleId = r.RoleId,
            RoleName = r.RoleName,
            IsActive = r.IsActive
        })
        .ToList();
    }
}