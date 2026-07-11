using Application.DTOs;
using Domain.Models;

namespace Application.Interface;

public interface IEmployeeRepository
{
    Task<EmployeeDto> EditEmployee(Employee? employee, CancellationToken ct);
    Task<long> GetSequenceId();
    Task<Employee> GetEmployeeById(long id, CancellationToken ct);
    Task<List<EmployeeDto>> GetAllEmployeesByCompanyId(long companyId, string viewOrder, int pageNumber, int pageSize,
        CancellationToken ct);
}