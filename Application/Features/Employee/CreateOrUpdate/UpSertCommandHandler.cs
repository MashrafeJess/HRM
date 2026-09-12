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
        Console.WriteLine($"[UpSertCommandHandler] Handle called. request.Dto.Id={request.Dto.Id}, Email={request.Dto.Email}");

        Domain.Models.Employee? employee = null;
        if (request.Dto.Id is null or 0)
        {
            Console.WriteLine($"[UpSertCommandHandler] Id is null/0 -> taking CREATE branch (a brand new employee row will be inserted).");
            employee = new Domain.Models.Employee
            {

                CreatedAt =  DateTime.Now,
                CompanyId = request.Dto.CompanyId,
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
            Console.WriteLine($"[UpSertCommandHandler] Id={request.Dto.Id} -> taking UPDATE branch (fetching existing employee to modify in place).");
            employee = await _authRepository.GetEmployeeByIdAsync(request.Dto.Id, cancellationToken) ?? throw new InvalidOperationException($"Employee with id {request.Dto.Id} does not exist.");
            Console.WriteLine($"[UpSertCommandHandler] Fetched existing employee EmployeeId={employee.EmployeeId} for update.");
            employee.FirstName = request.Dto.FirstName;
            employee.LastName = request.Dto.LastName;
            employee.DateOfBirth = request.Dto.DateOfBirth;
            employee.Email = request.Dto.Email;
            employee.DepartmentId = request.Dto.DepartmentId;
            employee.Salary = request.Dto.Salary;
            employee.Status = request.Dto.Status;
            employee.RoleId = request.Dto.RoleId ?? 0;
            employee.IsActive = request.Dto.IsActive ?? employee.IsActive;
            employee.JoinDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(request.Dto.Password))
            {
                employee.PasswordHash = new PasswordHasher<Domain.Models.Employee>().HashPassword(employee, request.Dto.Password);
            }
            employee.Phone = request.Dto.Phone;
            employee.CompanyId = request.Dto.CompanyId;
            employee.DateOfBirth = request.Dto.DateOfBirth;
            employee.Gender = request.Dto.Gender;
            employee.UpdatedAt = DateTime.UtcNow;
        }

        Console.WriteLine($"[UpSertCommandHandler] About to persist employee. employee.EmployeeId (pre-save)={employee.EmployeeId}");
        var result = await _repository.EditEmployee(employee, cancellationToken);
        Console.WriteLine($"[UpSertCommandHandler] Persist complete. result.Id={result.Id}");
        return result;
    }

    private async Task<string> GenerateEmployeeCode()
    {
        var seq = await _repository.GetSequenceId();
        Console.WriteLine($"[UpSertCommandHandler] GenerateEmployeeCode: GetSequenceId returned {seq}, generated code=EMP-{seq}");
        return $"EMP-{seq}";
    }
}