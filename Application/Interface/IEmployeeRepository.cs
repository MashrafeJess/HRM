using Application.DTOs;
using Domain.Models;

namespace Application.Interface;

public interface IEmployeeRepository
{
    Task<EmployeeDto> EditEmployee(Employee? employee, CancellationToken ct);
    Task<long> GetSequenceId();
}