namespace Application.DTOs;

public class DepartmentDto
{
    
    public long? DepartmentId { get; set; }

    public long CompanyId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
    public long? EmployeeCount { get; set; }
}