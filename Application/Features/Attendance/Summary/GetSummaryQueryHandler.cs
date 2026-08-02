using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Attendance.Summary;

public class GetSummaryQueryHandler(IAttendanceRepository repository) :
    IRequestHandler<GetAttendancesStatisticsByEmployeeId, AttendanceStatisticsDto>,
    IRequestHandler<GetAttendanceSummaryForMonth, AttendanceSummaryDto>,
    IRequestHandler<GetAttendanceSummaryForADay, AttendanceSummaryForADayDto>
{
    private readonly IAttendanceRepository _repository = repository;
    public async Task<AttendanceStatisticsDto> Handle(GetAttendancesStatisticsByEmployeeId request, CancellationToken cancellationToken)
    {
         var attendance = await _repository.GetAttendancesStatisticsByEmployeeId(request.EmployeeId, request.MonthId, request.YearId, cancellationToken);

         return new AttendanceStatisticsDto
         {
             PresentDays = attendance.PresentDays,
             LateDays = attendance.LateDays,
             LeaveDays = attendance.LeaveDays,
             AttendanceRatio = attendance.AttendanceRatio
         };
    }

    public async Task<AttendanceSummaryDto> Handle(GetAttendanceSummaryForMonth request,  CancellationToken cancellationToken)
    {
        var summary = await _repository.GetAttendanceSummaryForMonth(
            request.CompanyId,
            request.MonthId,
            request.YearId,
            cancellationToken);

        return new AttendanceSummaryDto
        {
            AverageAttendanceRate = summary.AverageAttendanceRate,
            TotalLateArrivals = summary.TotalLateArrivals,
            NumOfPerfectAttendance = summary.NumOfPerfectAttendance,
            EmployeeList = summary.EmployeeList,
            MostPunctualDepartmentId = summary.MostPunctualDepartmentId,
            MostPunctualDepartmentName = summary.MostPunctualDepartmentName,
            LateRate = summary.LateRate,
            HighestAbsenteeId = summary.HighestAbsenteeId,
            HighestAbsenteeName = summary.HighestAbsenteeName
        };
    }

    public async Task<AttendanceSummaryForADayDto> Handle(GetAttendanceSummaryForADay request, CancellationToken cancellationToken)
    {
        var attendanceSummary =
            await _repository.GetAttendanceSummaryForADay(request.CompanyId, request.Date, cancellationToken);

        return new AttendanceSummaryForADayDto
        {
            TotalEmployees = attendanceSummary.TotalEmployees,
            TotalPresent = attendanceSummary.TotalPresent,
            TotalLate = attendanceSummary.TotalLate,
            TotalLeave = attendanceSummary.TotalLeave,
            TotalAbsent = attendanceSummary.TotalAbsent,
            TotalAbsentArrival = attendanceSummary.TotalAbsentArrival,
        };
    }
}
