using Application.Common.Constants;
using Application.Common.Enums;
using Application.Interface;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Features.LeaveRequest.Approve;

public class ApproveLeaveRequestCommandHandler(
    ILeaveRepository leaveRepository,
    IAttendanceRepository attendanceRepository,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<ApproveLeaveRequestCommand, Unit>
{
    public async Task<Unit> Handle(
        ApproveLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        var leave = await leaveRepository.GetLeaveRequestById(request.LeaveRequestId, cancellationToken)
            ?? throw new KeyNotFoundException("Leave request not found.");

        var userIdValue = httpContextAccessor.HttpContext?.User.FindFirstValue("NameIdentifier");
        if (!long.TryParse(userIdValue, out var approverId))
        {
            throw new UnauthorizedAccessException("The authenticated user's employee ID was not found in the token.");
        }

        leave.Status = LeaveRequestStatusEnum.Approved.ToString();
        leave.ApprovedBy = approverId;
        leave.ApprovedAt = DateTime.UtcNow;

        await leaveRepository.UpdateLeaveRequest(leave, cancellationToken);

        var attendanceList = await attendanceRepository.GetAttendanceOnCertainRange(
            leave.EmployeeId,
            DateOnly.FromDateTime(leave.FromDate),
            DateOnly.FromDateTime(leave.ToDate),
            cancellationToken);

        for (var date = DateOnly.FromDateTime(leave.FromDate);
             date <= DateOnly.FromDateTime(leave.ToDate);
             date = date.AddDays(1))
        {
            if (WeeklyHolidayCalendar.IsHoliday(date))
            {
                continue;
            }

            var attendance = attendanceList.FirstOrDefault(a => a.AttendanceDate == date)
                ?? new Domain.Models.Attendance
                {
                    CompanyId = leave.CompanyId,
                    EmployeeId = leave.EmployeeId,
                    AttendanceDate = date,
                    CreatedAt = DateTime.UtcNow
                };

            attendance.Status = "On Leave";
            attendance.Remarks = leave.Reason;
            await attendanceRepository.CreateOrUpdateAttendance(attendance, cancellationToken);
        }

        return Unit.Value;
    }
}
