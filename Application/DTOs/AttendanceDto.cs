namespace Application.DTOs;

public class AttendanceDto
{
    public long? AttendanceId { get; set; }

    public long CompanyId { get; set; }

    public long EmployeeId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public TimeOnly? CheckIn { get; set; }

    public TimeOnly? CheckOut { get; set; }

    public decimal? WorkingHours { get; set; }

    public int? LateMinutes { get; set; }

    public int? EarlyLeaveMinutes { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public DateTime? CreatedAt { get; set; }
}