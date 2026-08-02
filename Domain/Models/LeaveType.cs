namespace Domain.Models;

public partial class LeaveType
{
    public long LeaveTypeId { get; set; }

    public string LeaveTypeName { get; set; } = null!;

    public string? LeaveTypeDescription { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
