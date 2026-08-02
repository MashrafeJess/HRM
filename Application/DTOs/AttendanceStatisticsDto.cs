namespace Application.DTOs;

public class AttendanceStatisticsDto
{
    public long PresentDays { get; set; }
    public long LateDays { get; set; }
    public long LeaveDays { get; set; }
    public long AttendanceRatio { get; set; }
}

public class PerfectAttendanceEmployeeSummaryDto
{
    public long EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }
    public long? TotalAbsent { get; set; }
}

public class AttendanceSummaryDto
{
    public long? AverageAttendanceRate { get; set; }
    public long? TotalLateArrivals { get; set; }
    public long? NumOfPerfectAttendance { get; set; }
    public List<PerfectAttendanceEmployeeSummaryDto>? EmployeeList { get; set; } 
    public long? MostPunctualDepartmentId { get; set; }
    public string? MostPunctualDepartmentName { get; set; }
    public decimal? LateRate { get; set; }
    public long? HighestAbsenteeId { get; set; }
    public string? HighestAbsenteeName { get; set; }
}

public class AttendanceSummaryForADayDto
{
    public long TotalEmployees { get; set; }
    public long TotalPresent { get; set; }
    public long TotalLate { get; set; }
    public long TotalLeave { get; set; }
    public long TotalAbsent { get; set; }
    public long TotalAbsentArrival { get; set; }
}
