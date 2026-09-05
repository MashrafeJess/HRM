using Domain.Models;

namespace Application.Interface;

public interface IDepartmentRepository
{
    Task<Department> UpSertDepartment(Department department, CancellationToken cancellationToken);
    Task<(List<Department> Departments, int TotalCount)> GetAllDepartments(long companyId, string? viewOrder, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(Department, int)> GetByDepartmentId(long departmentId, CancellationToken cancellationToken);
}