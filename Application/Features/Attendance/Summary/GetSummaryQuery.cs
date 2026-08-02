using Application.DTOs;
using MediatR;

namespace Application.Features.Attendance.Summary;

public  record GetAttendancesStatisticsByEmployeeId(long EmployeeId, int MonthId, int YearId, CancellationToken Ct) : IRequest<AttendanceStatisticsDto>;

public record GetAttendanceSummaryForMonth(long CompanyId, int MonthId, long YearId, CancellationToken Ct) : IRequest<AttendanceSummaryDto>;

public record GetAttendanceSummaryForADay(long CompanyId, DateOnly Date, CancellationToken Ct) : IRequest<AttendanceSummaryForADayDto>;





