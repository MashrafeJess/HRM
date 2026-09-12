using Application.Common.Exceptions;
using Application.DTOs;
using Application.Interface;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EmployeeRepository(IAppDbContext appDbContext) : IEmployeeRepository
{
    private readonly IAppDbContext _appDbContext = appDbContext;

    public async Task<EmployeeDto> EditEmployee(Employee? employee, CancellationToken ct)
    {
        Console.WriteLine($"[EmployeeRepository] EditEmployee called. employee.EmployeeId={employee?.EmployeeId}");

        // BUG FIX: employee.EmployeeId is a non-nullable `long`, so the old check
        // `employee?.EmployeeId == null` could only ever be true if `employee` itself
        // was null — it was NEVER true for a genuinely new employee (EmployeeId == 0),
        // meaning this always fell into the Update branch. It "worked" for creates only
        // because EF Core's change tracker separately auto-detects Added state for an
        // untracked entity whose store-generated key equals the CLR default (0). Checking
        // EmployeeId == 0 directly expresses the actual intent instead of relying on that
        // incidental EF behavior.
        if (employee!.EmployeeId == 0)
        {
            Console.WriteLine("[EmployeeRepository] EmployeeId == 0 -> AddAsync (insert new row).");
            await _appDbContext.Employees.AddAsync(employee, ct);
        }
        else
        {
            Console.WriteLine($"[EmployeeRepository] EmployeeId={employee.EmployeeId} -> Update (modify existing row).");
            _appDbContext.Employees.Update(employee);
        }

        await _appDbContext.SaveChangesAsync(ct);
        Console.WriteLine($"[EmployeeRepository] SaveChangesAsync complete. employee.EmployeeId (post-save)={employee.EmployeeId}");
        return new EmployeeDto
        {
            Id = employee.EmployeeId,
            EmployeeCode = employee.EmployeeCode,
            DepartmentId = employee.DepartmentId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            DateOfBirth = employee.DateOfBirth,
            Gender = employee.Gender,
            CompanyId = employee.CompanyId,
            IsActive = employee.IsActive,
            JoinDate = employee.JoinDate,
            Phone = employee.Phone,
            Salary = employee.Salary,
            Status = employee.Status
        };
    }

    public async Task<long> GetSequenceId()
    {
        var id = await _appDbContext.Employees.OrderByDescending(x => x.EmployeeId).Select(s => s.EmployeeId)
            .FirstOrDefaultAsync();
        Console.WriteLine($"[EmployeeRepository] GetSequenceId returning {id}");
        return id;
    }

    public async Task<Employee> GetEmployeeById(long id, CancellationToken ct)
    {
        Console.WriteLine($"[EmployeeRepository] GetEmployeeById called with id={id}");
        // Resolve Role separately instead of .Include(e => e.Role): RoleId is a
        // non-nullable FK with no DB constraint enforcing it, so an orphaned RoleId
        // (pointing at a deleted/missing role) must not hide an otherwise-valid,
        // active employee behind an inner join.
        var employee = await _appDbContext.Employees
            .AsNoTracking()
            .Where(e => e.EmployeeId == id && e.IsActive == true)
            .FirstOrDefaultAsync(ct);

        if (employee is not null)
        {
            employee.Role = await _appDbContext.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoleId == employee.RoleId, ct);
        }
        Console.WriteLine(employee is null
            ? $"[EmployeeRepository] GetEmployeeById: no active employee found for id={id}"
            : $"[EmployeeRepository] GetEmployeeById: found EmployeeId={employee.EmployeeId}, FirstName={employee.FirstName}");
        return employee ?? throw new NotFoundException("Employee not found");
    }

    public async Task<PagedResult<EmployeeDto>> GetAllEmployeesByCompanyId(long companyId, long? departmentId, string viewOrder, int pageNumber, int pageSize, CancellationToken ct)
    {
        Console.WriteLine($"[EmployeeRepository] GetAllEmployeesByCompanyId called. companyId={companyId}, departmentId={departmentId}, viewOrder={viewOrder}, pageNumber={pageNumber}, pageSize={pageSize}");

        var employees = _appDbContext.Employees
            .AsNoTracking()
            .Where(e => e.CompanyId == companyId && (departmentId == null || e.DepartmentId == departmentId) && e.IsActive == true);

        employees = viewOrder.ToLower()switch
        {
            "desc" => employees.OrderByDescending(c => c.CreatedAt),
            _ => employees.OrderBy(c => c.CreatedAt)
        };

        var totalCount = await employees.CountAsync(ct);

        var items = await employees
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            // Resolve RoleName via an explicit correlated subquery instead of the
            // `e.Role` navigation: RoleId is a required FK with no DB constraint
            // enforcing it, so an orphaned RoleId would otherwise be translated into
            // an inner join and silently drop the employee from the page entirely
            // (see the same issue/fix in GetEmployeeById above).
            .Select(e=> new EmployeeDto
            {
                Id = e.EmployeeId,
                CompanyId = e.CompanyId,
                DepartmentId = e.DepartmentId,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                RoleId = e.RoleId,
                RoleName = _appDbContext.Roles
                    .Where(r => r.RoleId == e.RoleId)
                    .Select(r => r.RoleName)
                    .FirstOrDefault(),
                Gender = e.Gender,
                DateOfBirth = e.DateOfBirth,
                JoinDate = e.JoinDate,
                Salary = e.Salary,
                Status = e.Status,
                IsActive = e.IsActive
            })
            .ToListAsync(ct);

        Console.WriteLine($"[EmployeeRepository] GetAllEmployeesByCompanyId returning {items.Count} item(s), ids=[{string.Join(", ", items.Select(i => i.Id))}]");

        return new PagedResult<EmployeeDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
