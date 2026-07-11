using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Employee.GetById;

public class GetEmployeeQueryHandler(IEmployeeRepository employeeRepository) : IRequestHandler<GetEmployeeQuery, EmployeeDto>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    public async Task<EmployeeDto> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var employee = await _employeeRepository.GetEmployeeById(request.EmployeeId, cancellationToken);
            return new EmployeeDto
            {
                CompanyId = employee.CompanyId,
                DepartmentId = employee.DepartmentId,
                EmployeeCode = employee.EmployeeCode,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Phone = employee.Phone,
                RoleName = employee.Role?.RoleName,
                Gender = employee.Gender,
                DateOfBirth = employee.DateOfBirth,
                JoinDate = employee.JoinDate,
                Salary = employee.Salary,
                Status = employee.Status,
                IsActive = employee.IsActive
            };
        }
        catch(Exception ex)
        {
            throw new ApplicationException($"Error getting employee with id {request.EmployeeId}", ex);            
        }
    }
}