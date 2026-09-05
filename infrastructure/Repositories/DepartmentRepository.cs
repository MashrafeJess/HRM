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

    public async Task<(List<Department> Departments, int TotalCount)> GetAllDepartments(long companyId, string? viewOrder, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Departments.Where(d => d.CompanyId == companyId);
            query = viewOrder?.ToLower() switch
            {
                "desc" => query.OrderByDescending(d => d.CreatedAt),
                _ => query.OrderBy(d => d.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var departments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (departments, totalCount);
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