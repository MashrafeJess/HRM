using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Role.GetById;

public class GetRoleByIdQueryHandler(IRoleRepository repository) : IRequestHandler<GetRoleByIdQuery, RoleDto>
{
    private readonly IRoleRepository _repository = repository;
    public async Task<RoleDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _repository.GetRoleById(request.Id, cancellationToken);
        return new RoleDto
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            IsActive = role.IsActive
        };
    }
}