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
        if (employee?.EmployeeId == null)
        {
            await _appDbContext.Employees.AddAsync(employee!, ct);
        }
        else
        {
            _appDbContext.Employees.Update(employee);
        }

        await _appDbContext.SaveChangesAsync(ct);
        return new EmployeeDto
        {
            EmployeeCode = employee!.EmployeeCode,
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
        return id;
    }

    public async Task<Employee> GetEmployeeById(long id, CancellationToken ct)
    {
        var employee = await _appDbContext.Employees
            .Include((e=>e.Role))
            .Where(e=>e.EmployeeId==id && e.IsActive == true).FirstOrDefaultAsync(ct);
        return employee ?? throw new NotFoundException("Employee not found");
    }

    public async Task<PagedResult<EmployeeDto>> GetAllEmployeesByCompanyId(long companyId, long? departmentId, string viewOrder, int pageNumber, int pageSize, CancellationToken ct)
    {
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
            .Select(e=> new EmployeeDto
            {
                CompanyId = e.CompanyId,
                DepartmentId = e.DepartmentId,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                RoleName = e.Role != null ? e.Role.RoleName : null,
                Gender = e.Gender,
                DateOfBirth = e.DateOfBirth,
                JoinDate = e.JoinDate,
                Salary = e.Salary,
                Status = e.Status,
                IsActive = e.IsActive
            })
            .ToListAsync(ct);

        return new PagedResult<EmployeeDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
