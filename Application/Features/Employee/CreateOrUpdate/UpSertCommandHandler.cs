using Application.DTOs;
using Application.Interface;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Employee.CreateOrUpdate;

public class UpSertCommandHandler(IEmployeeRepository repository, IAuthRepository authRepository) : IRequestHandler<CreateOrUpdateUpSertCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _repository = repository;
    private readonly IAuthRepository _authRepository = authRepository;
    
    public async Task<EmployeeDto> Handle(CreateOrUpdateUpSertCommand request, CancellationToken cancellationToken)
    {
        Domain.Models.Employee? employee = null;
        if (request.Dto.Id is null or 0)
        {
            employee = new Domain.Models.Employee
            {
                
                CreatedAt =  DateTime.Now,
                EmployeeCode = await GenerateEmployeeCode(),
                DateOfBirth =  request.Dto.DateOfBirth,
                Email = request.Dto.Email,      
                FirstName = request.Dto.FirstName,
                LastName = request.Dto.LastName,
                DepartmentId =  request.Dto.DepartmentId,
                Gender =  request.Dto.Gender,
                IsActive = true,
                JoinDate = DateTime.UtcNow,
                PasswordHash = new PasswordHasher<Domain.Models.Employee>().HashPassword(employee!, request.Dto.Password ?? throw new ArgumentNullException(nameof(request.Dto.Password))),
                Phone =  request.Dto.Phone,
                Salary =  request.Dto.Salary,
                Status =  request.Dto.Status,
                RoleId = request.Dto.RoleId ?? 0,
            };
        }
        else
        {
            employee = await _authRepository.GetEmployeeByIdAsync(request.Dto.Id, cancellationToken) ?? throw new InvalidOperationException($"Employee with id {request.Dto.Id} does not exist.");
            employee.FirstName = request.Dto.FirstName;
            employee.LastName = request.Dto.LastName;
            employee.DateOfBirth = request.Dto.DateOfBirth;
            employee.Email = request.Dto.Email;
            employee.DepartmentId = request.Dto.DepartmentId;
            employee.Salary = request.Dto.Salary;
            employee.Status = request.Dto.Status;
            employee.RoleId = request.Dto.RoleId ?? 0;
            employee.IsActive = request.Dto.IsActive ?? false;
            employee.JoinDate = DateTime.UtcNow;
            employee.PasswordHash = new PasswordHasher<Domain.Models.Employee>().HashPassword(employee, request.Dto.Password ?? throw new ArgumentNullException(nameof(request.Dto.Password)));
            employee.Phone = request.Dto.Phone;
            employee.CompanyId = request.Dto.CompanyId;
            employee.DateOfBirth = request.Dto.DateOfBirth;
            employee.Gender = request.Dto.Gender;
            employee.UpdatedAt = DateTime.UtcNow;
        }

        var result = await _repository.EditEmployee(employee, cancellationToken);
        return result;
    }

    private async Task<string> GenerateEmployeeCode()
    {
        var seq = await _repository.GetSequenceId();
        return $"EMP-{seq}";
    }
}