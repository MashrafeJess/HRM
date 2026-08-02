using Application.DTOs;
using Domain.Models;

namespace Application.Interface;

public interface IAttendanceRepository
{
    public Task<Attendance?> CreateOrUpdateAttendance(Attendance attendance, CancellationToken ct); 
    public Task<List<Attendance>> GetAttendancesByEmployeeId(long employeeId, CancellationToken ct); 
    public Task<List<Attendance>> GetAttendanceByDate(long companyId, DateOnly date, CancellationToken ct); 

    public Task<AttendanceStatisticsDto> GetAttendancesStatisticsByEmployeeId(long employeeId, long monthId,
        long yearId, CancellationToken ct); 

    public Task<AttendanceSummaryDto> GetAttendanceSummaryForMonth(long companyId, long monthId, long yearId,
        CancellationToken ct); 

    public Task<AttendanceSummaryForADayDto> GetAttendanceSummaryForADay(long companyId, DateOnly date,
        CancellationToken ct); 

    public Task MarkAbsentEmployeeAsync(DateOnly date, CancellationToken ct);

    public Task<Attendance> GetAttendanceById(long? attendanceId, CancellationToken ct);

    public Task<List<Attendance>> GetAttendanceOnCertainRange(long employeeId, DateOnly fromDate, DateOnly toDate,
        CancellationToken ct);
}