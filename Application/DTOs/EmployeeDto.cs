using MediatR;

namespace Application.DTOs;

public class EmployeeDto : IRequest
{
    public long? Id { get; set; } = 0;
    
    public long CompanyId { get; set; }

    public long DepartmentId { get; set; }

    public string? EmployeeCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public long RoleId { get; set; }

    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public DateTime JoinDate { get; set; }

    public decimal Salary { get; set; }

    public string Status { get; set; } = null!;
    
    public bool? IsActive { get; set; } = false;
}