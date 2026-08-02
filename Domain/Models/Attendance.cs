namespace Domain.Models;

public partial class Attendance
{
    public long AttendanceId { get; set; }

    public long CompanyId { get; set; }

    public long EmployeeId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public TimeOnly? CheckIn { get; set; }

    public TimeOnly? CheckOut { get; set; }

    public decimal? WorkingHours { get; set; }

    public TimeOnly? LateMinutes { get; set; }

    public TimeOnly? EarlyLeaveMinutes { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }
}
