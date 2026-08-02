namespace Domain.Models;

public partial class LeaveRequest
{
    public long LeaveRequestId { get; set; }

    public long CompanyId { get; set; }

    public long EmployeeId { get; set; }

    public long LeaveTypeId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public int TotalDays { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public long? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? Airecommendation { get; set; }

    public string? Ainotes { get; set; }

    public DateTime? CreatedAt { get; set; }

}
