using Application.Interface;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RoleRepository(IAppDbContext context) : IRoleRepository
{
    private readonly IAppDbContext _context = context;

    public async Task<Role> EditRole(Role role, CancellationToken ct)
    {
        if (role.RoleId == 0)
        {
            await _context.Roles.AddAsync(role, ct);
        }
        else
        {
            _context.Roles.Update(role);
        }

        await _context.SaveChangesAsync(ct);
        return role;
    }

    public async Task<List<Role>> GetAllRoles()
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.RoleName)
            .ToListAsync();
    }

    public async Task<Role> GetRoleById(long roleId, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.RoleId == roleId, cancellationToken);

        return role ?? throw new Application.Common.Exceptions.NotFoundException("Role not found");
    }
}
