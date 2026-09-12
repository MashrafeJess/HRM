using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Attendance.CheckIn_CheckOut;

public class CheckInCommandHandler(IAttendanceRepository repository)
    : IRequestHandler<CheckInCommand, AttendanceDto>
{
    private static readonly TimeOnly ShiftStart = new(9, 0);
    private static readonly TimeOnly ShiftEnd = new(18, 0);
    private readonly IAttendanceRepository _repository = repository;

    public async Task<AttendanceDto> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var now = DateTime.Now;
        var currentTime = TimeOnly.FromDateTime(now);
        var attendanceDate = dto.AttendanceDate == default
            ? DateOnly.FromDateTime(now)
            : dto.AttendanceDate;

        // Resolve any existing row for this employee/date before deciding what this
        // call means — the daily mark-absent job may have already inserted an
        // "Absent" row (with no CheckIn) before the employee actually checks in, so
        // "an attendance row already exists" must NOT be read as "this is a check-out".
        var attendance = dto.AttendanceId is null or 0
            ? await _repository.GetAttendanceForEmployeeOnDate(dto.EmployeeId, attendanceDate, cancellationToken)
            : await _repository.GetAttendanceById(dto.AttendanceId, cancellationToken);

        if (attendance is null)
        {
            // No row yet for this employee/date: first check-in of the day.
            attendance = new Domain.Models.Attendance
            {
                CompanyId = dto.CompanyId,
                EmployeeId = dto.EmployeeId,
                AttendanceDate = attendanceDate,
                CheckIn = currentTime,
                Status = GetStatus(currentTime),
                LateMinutes = (currentTime <= ShiftStart) ? new TimeOnly(0,0) : GetDuration(currentTime - ShiftStart),
                CreatedAt = now
            };
        }
        else if (attendance.CheckIn is null)
        {
            // A row already exists (e.g. auto-marked "Absent") but the employee
            // hasn't actually checked in yet today — this is still a check-in.
            attendance.CompanyId = dto.CompanyId;
            attendance.EmployeeId = dto.EmployeeId;
            attendance.CheckIn = currentTime;
            attendance.Status = GetStatus(currentTime);
            attendance.LateMinutes = (currentTime <= ShiftStart) ? new TimeOnly(0,0) : GetDuration(currentTime - ShiftStart);
        }
        else if (attendance.CheckOut is null)
        {
            // Already checked in, not yet checked out — this is a check-out.
            attendance.CompanyId = dto.CompanyId;
            attendance.EmployeeId = dto.EmployeeId;
            attendance.CheckOut = currentTime;
            attendance.EarlyLeaveMinutes = GetDuration(ShiftEnd - currentTime);
            attendance.WorkingHours = Math.Round(
                (decimal)(currentTime - attendance.CheckIn.Value).TotalHours, 2);
        }
        else
        {
            throw new InvalidOperationException("Already checked in and checked out for this date.");
        }

        await _repository.CreateOrUpdateAttendance(attendance, cancellationToken);
        return MapToDto(attendance);
    }

    private static string GetStatus(TimeOnly checkIn) =>
        checkIn > ShiftStart ? "Late" : "Present";

    private static TimeOnly? GetDuration(TimeSpan duration) =>
        duration > TimeSpan.Zero ? TimeOnly.FromTimeSpan(duration) : null;

    private static AttendanceDto MapToDto(Domain.Models.Attendance attendance) => new()
    {
        AttendanceId = attendance.AttendanceId,
        CompanyId = attendance.CompanyId,
        EmployeeId = attendance.EmployeeId,
        AttendanceDate = attendance.AttendanceDate,
        CheckIn = attendance.CheckIn,
        CheckOut = attendance.CheckOut,
        WorkingHours = attendance.WorkingHours,
        LateMinutes = ToMinutes(attendance.LateMinutes),
        EarlyLeaveMinutes = ToMinutes(attendance.EarlyLeaveMinutes),
        Status = attendance.Status,
        Remarks = attendance.Remarks,
        CreatedAt = attendance.CreatedAt
    };

    private static int? ToMinutes(TimeOnly? duration) =>
        duration is null ? null : (int)duration.Value.ToTimeSpan().TotalMinutes;
}
