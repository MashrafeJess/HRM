using System.ComponentModel;
using Application.Common;
using Application.Common.Enums;
using Application.Common.Exceptions;
using Application.DTOs;
using Application.Interface;
using Domain.Models;

namespace Infrastructure.Services.Ai;

// Every tool is scoped to the caller's company via the constructor, never via a
// parameter, so the model has no way to reach another tenant's data.
internal sealed class HrAssistantTools(
    long companyId,
    IAttendanceRepository attendanceRepository,
    ILeaveRepository leaveRepository,
    IEmployeeRepository employeeRepository)
{
    [Description("Find employees of the company by partial name or employee code. Returns employeeId, full name, employee code, departmentId and status. Call this first whenever a question mentions a person by name, to get their employeeId.")]
    public async Task<object> FindEmployees(
        [Description("Optional case-insensitive text matched against first name, last name or employee code. Leave empty to list all employees.")] string? search,
        CancellationToken ct)
    {
        var all = await GetCompanyEmployeesAsync(ct);

        var matches = string.IsNullOrWhiteSpace(search)
            ? all
            : all.Where(e =>
                    (e.FirstName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.LastName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    ($"{e.FirstName} {e.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (e.EmployeeCode?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        return matches.Select(e => new
        {
            employeeId = e.Id,
            fullName = $"{e.FirstName} {e.LastName}".Trim(),
            employeeCode = e.EmployeeCode,
            departmentId = e.DepartmentId,
            status = e.Status,
            isActive = e.IsActive
        }).ToList();
    }

    [Description("Company-wide attendance totals for one day: total employees, present, late, on leave, absent.")]
    public async Task<object> GetAttendanceSummaryForDay(
        [Description("Date in yyyy-MM-dd format")] string date,
        CancellationToken ct)
    {
        if (!TryParseDate(date, out var day, out var error))
        {
            return new { error };
        }

        return await attendanceRepository.GetAttendanceSummaryForADay(companyId, day, ct);
    }

    [Description("Company-wide attendance summary for a whole month: average attendance rate, total late arrivals, employees with perfect attendance (with their absent counts), most punctual department and its late rate, and the employee with the most absences.")]
    public async Task<object> GetAttendanceSummaryForMonth(
        [Description("Month number, 1 to 12")] int month,
        [Description("Four-digit year, e.g. 2026")] int year,
        CancellationToken ct)
    {
        if (month is < 1 or > 12)
        {
            return new { error = "month must be between 1 and 12." };
        }

        return await attendanceRepository.GetAttendanceSummaryForMonth(companyId, month, year, ct);
    }

    [Description("Per-employee attendance counts for a whole month: one row per active employee with present, late, absent and leave days plus total minutes late, sorted by most late days first. Use this for questions like who was late or absent the most, who has the best attendance, ranking or comparing employees, or how many times a named person was late in a month.")]
    public async Task<object> GetMonthlyAttendanceByEmployee(
        [Description("Month number, 1 to 12")] int month,
        [Description("Four-digit year, e.g. 2026")] int year,
        CancellationToken ct)
    {
        if (month is < 1 or > 12)
        {
            return new { error = "month must be between 1 and 12." };
        }

        var rows = await attendanceRepository.GetMonthlyAttendanceByEmployee(companyId, month, year, ct);

        return rows.Select(r => new
        {
            r.EmployeeId,
            r.EmployeeName,
            r.DepartmentName,
            r.PresentDays,
            r.LateDays,
            r.AbsentDays,
            r.LeaveDays,
            totalLate = FormatDuration(r.TotalLateMinutes)
        }).ToList();
    }

    [Description("Every attendance record of the company for one day: one row per employee with name, check-in and check-out time, status (Present, Late, Absent, On Leave) and minutes late.")]
    public async Task<object> GetAttendanceRecordsForDay(
        [Description("Date in yyyy-MM-dd format")] string date,
        CancellationToken ct)
    {
        if (!TryParseDate(date, out var day, out var error))
        {
            return new { error };
        }

        var records = await attendanceRepository.GetAttendanceByDate(companyId, day, ct);
        var names = await GetEmployeeNameLookupAsync(ct);

        return records.Select(a => ToAttendanceRow(a, names.GetValueOrDefault(a.EmployeeId))).ToList();
    }

    [Description("Attendance statistics of one employee for a month: present days, late days, leave days and attendance ratio in percent.")]
    public async Task<object> GetEmployeeAttendanceStatistics(
        [Description("The employeeId (get it from find_employees)")] long employeeId,
        [Description("Month number, 1 to 12")] int month,
        [Description("Four-digit year, e.g. 2026")] int year,
        CancellationToken ct)
    {
        if (month is < 1 or > 12)
        {
            return new { error = "month must be between 1 and 12." };
        }

        var employee = await GetCompanyEmployeeAsync(employeeId, ct);
        if (employee is null)
        {
            return new { error = $"No active employee with id {employeeId} in this company." };
        }

        var stats = await attendanceRepository.GetAttendancesStatisticsByEmployeeId(employeeId, month, year, ct);

        return new
        {
            employeeId,
            employeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            month,
            year,
            stats.PresentDays,
            stats.LateDays,
            stats.LeaveDays,
            attendanceRatioPercent = stats.AttendanceRatio
        };
    }

    [Description("Day-by-day attendance records of one employee between two dates, inclusive.")]
    public async Task<object> GetEmployeeAttendanceRecords(
        [Description("The employeeId (get it from find_employees)")] long employeeId,
        [Description("Start date in yyyy-MM-dd format")] string fromDate,
        [Description("End date in yyyy-MM-dd format")] string toDate,
        CancellationToken ct)
    {
        if (!TryParseDate(fromDate, out var from, out var error) || !TryParseDate(toDate, out var to, out error))
        {
            return new { error };
        }

        if (from > to)
        {
            return new { error = "fromDate must not be after toDate." };
        }

        var employee = await GetCompanyEmployeeAsync(employeeId, ct);
        if (employee is null)
        {
            return new { error = $"No active employee with id {employeeId} in this company." };
        }

        var records = await attendanceRepository.GetAttendanceOnCertainRange(employeeId, from, to, ct);
        var name = $"{employee.FirstName} {employee.LastName}".Trim();

        return records.OrderBy(a => a.AttendanceDate).Select(a => ToAttendanceRow(a, name)).ToList();
    }

    [Description("Leave requests of the company filtered by status, with employee names, dates, total days, reason and approval info.")]
    public async Task<object> GetLeaveRequestsByStatus(
        [Description("One of: All, Pending, Approved, Rejected, Cancelled")] string status,
        CancellationToken ct)
    {
        if (!Enum.TryParse<LeaveRequestStatusEnum>(status, ignoreCase: true, out var parsed))
        {
            return new { error = "status must be one of: All, Pending, Approved, Rejected, Cancelled." };
        }

        var requests = await leaveRepository.LeaveRequestsByStatus(parsed, companyId, ct);
        var names = await GetEmployeeNameLookupAsync(ct);

        return requests
            .Where(l => l.CompanyId == companyId)
            .Select(l => ToLeaveRow(l, names.GetValueOrDefault(l.EmployeeId), names))
            .ToList();
    }

    [Description("Full leave request history of one employee (all statuses).")]
    public async Task<object> GetEmployeeLeaveHistory(
        [Description("The employeeId (get it from find_employees)")] long employeeId,
        CancellationToken ct)
    {
        var employee = await GetCompanyEmployeeAsync(employeeId, ct);
        if (employee is null)
        {
            return new { error = $"No active employee with id {employeeId} in this company." };
        }

        var history = await leaveRepository.LeaveHistoryByEmployeeId(employeeId, ct);
        var names = await GetEmployeeNameLookupAsync(ct);
        var name = $"{employee.FirstName} {employee.LastName}".Trim();

        return history
            .Where(l => l.CompanyId == companyId)
            .OrderByDescending(l => l.FromDate)
            .Select(l => ToLeaveRow(l, name, names))
            .ToList();
    }

    private async Task<Employee?> GetCompanyEmployeeAsync(long employeeId, CancellationToken ct)
    {
        try
        {
            var employee = await employeeRepository.GetEmployeeById(employeeId, ct);
            return employee.CompanyId == companyId ? employee : null;
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    private async Task<List<EmployeeDto>> GetCompanyEmployeesAsync(CancellationToken ct)
    {
        var all = new List<EmployeeDto>();
        var page = 1;

        while (true)
        {
            var result = await employeeRepository.GetAllEmployeesByCompanyId(
                companyId, null, "asc", page, PaginationDefaults.MaxPageSize, ct);

            all.AddRange(result.Items);

            if (page >= result.TotalPages || result.Items.Count == 0)
            {
                break;
            }

            page++;
        }

        return all;
    }

    private async Task<Dictionary<long, string>> GetEmployeeNameLookupAsync(CancellationToken ct)
    {
        var all = await GetCompanyEmployeesAsync(ct);
        return all
            .Where(e => e.Id.HasValue)
            .ToDictionary(e => e.Id!.Value, e => $"{e.FirstName} {e.LastName}".Trim());
    }

    private static object ToAttendanceRow(Attendance a, string? employeeName) => new
    {
        a.EmployeeId,
        employeeName,
        date = a.AttendanceDate.ToString("yyyy-MM-dd"),
        checkIn = a.CheckIn?.ToString("HH:mm"),
        checkOut = a.CheckOut?.ToString("HH:mm"),
        a.Status,
        late = FormatDuration(ToMinutes(a.LateMinutes)),
        earlyLeave = FormatDuration(ToMinutes(a.EarlyLeaveMinutes)),
        a.WorkingHours,
        a.Remarks
    };

    private static string? FormatDuration(int? minutes)
    {
        if (minutes is null or 0)
        {
            return minutes is null ? null : "0m";
        }

        var hours = minutes.Value / 60;
        var mins = minutes.Value % 60;
        return hours == 0 ? $"{mins}m" : $"{hours}h {mins}m";
    }

    private static object ToLeaveRow(LeaveRequest l, string? employeeName, Dictionary<long, string> names) => new
    {
        l.LeaveRequestId,
        l.EmployeeId,
        employeeName,
        l.LeaveTypeId,
        fromDate = l.FromDate.ToString("yyyy-MM-dd"),
        toDate = l.ToDate.ToString("yyyy-MM-dd"),
        l.TotalDays,
        l.Reason,
        l.Status,
        approvedBy = l.ApprovedBy,
        approvedByName = l.ApprovedBy.HasValue ? names.GetValueOrDefault(l.ApprovedBy.Value) : null,
        approvedAt = l.ApprovedAt?.ToString("yyyy-MM-dd")
    };

    private static int? ToMinutes(TimeOnly? duration)
        => duration is null ? null : (int)duration.Value.ToTimeSpan().TotalMinutes;

    private static bool TryParseDate(string value, out DateOnly date, out string error)
    {
        if (DateOnly.TryParseExact(value, "yyyy-MM-dd", out date))
        {
            error = string.Empty;
            return true;
        }

        error = $"'{value}' is not a valid date. Use yyyy-MM-dd.";
        return false;
    }
}
