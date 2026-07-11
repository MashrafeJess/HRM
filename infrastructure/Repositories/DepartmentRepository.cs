using Application.Common.Exceptions;
using Application.Interface;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DepartmentRepository(IAppDbContext context) : IDepartmentRepository
{
    private readonly IAppDbContext _context = context;

    public async Task<Department> UpSertDepartment(Department department,CancellationToken cancellationToken)
    {
        try
        {
            if (department.DepartmentId == 0)
            {
                await _context.Departments.AddAsync(department, cancellationToken);
            }
            else
            {
                _context.Departments.Update(department);
            }
            await _context.SaveChangesAsync(cancellationToken);
            
            return department;
        }
        catch (Exception e)
        {
            throw new BadRequestException("The issue is " + e.Message);
        }
    }

    public async Task<List<Department>> GetAllDepartments(long companyId, CancellationToken cancellationToken)
    {
        try
        {
            var departments = await _context.Departments.Where(d => d.CompanyId == companyId).ToListAsync(cancellationToken);
            return departments;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<(Department, int)> GetByDepartmentId(long departmentId, CancellationToken cancellationToken)
    {
        try
        {
            var department = await _context.Departments
                .AsNoTracking()
                .Where(d => d.DepartmentId == departmentId)
                .Select(d=> new
                    {
                       Department = d,
                       EmployeeCount = _context.Employees.Count(e => e.DepartmentId == d.DepartmentId)
                    })
                .FirstOrDefaultAsync(cancellationToken);
            return department == null ? throw new NotFoundException("Department not found") : (department.Department, department.EmployeeCount);
        }
        catch(Exception e)
        {
            throw new BadRequestException("The issue is " + e.Message);
        }
    }
}