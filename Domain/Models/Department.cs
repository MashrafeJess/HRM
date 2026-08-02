namespace Domain.Models;

public partial class Department
{
    public long DepartmentId { get; set; }

    public long CompanyId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
