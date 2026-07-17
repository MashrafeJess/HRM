using Domain.Models;

namespace Application.Interface;

public interface IRoleRepository
{
    Task<Role> EditRole(Role role, CancellationToken ct);
    Task<List<Role>> GetAllRoles();
    Task<Role> GetRoleById(long roleId, CancellationToken cancellationToken);
}
