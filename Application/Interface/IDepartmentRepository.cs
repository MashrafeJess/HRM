using Domain.Models;

namespace Application.Interface;

public interface IDepartmentRepository
{
    Task<Department> UpSertDepartment(Department department, CancellationToken cancellationToken);
    Task<List<Department>> GetAllDepartments(long companyId, CancellationToken cancellationToken);
    Task<(Department, int)> GetByDepartmentId(long departmentId, CancellationToken cancellationToken);
}