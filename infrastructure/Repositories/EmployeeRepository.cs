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
            RoleId = employee.RoleId,
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
}
