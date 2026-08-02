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
        Domain.Models.Attendance attendance;

        if (dto.AttendanceId is null or 0)
        {
            attendance = new Domain.Models.Attendance
            {
                CompanyId = dto.CompanyId,
                EmployeeId = dto.EmployeeId,
                AttendanceDate = dto.AttendanceDate == default
                    ? DateOnly.FromDateTime(now)
                    : dto.AttendanceDate,
                CheckIn = currentTime,
                Status = GetStatus(currentTime),
                LateMinutes = (currentTime <= ShiftStart) ? new TimeOnly(0,0) : GetDuration(currentTime - ShiftStart),
                CreatedAt = now
            };
        }
        else
        {
            attendance = await _repository.GetAttendanceById(dto.AttendanceId, cancellationToken);

            if (attendance.CheckIn is null)
            {
                throw new InvalidOperationException("Cannot check out before checking in.");
            }

            attendance.AttendanceId = dto.AttendanceId ?? 0;
            attendance.CompanyId =  dto.CompanyId;
            attendance.EmployeeId = dto.EmployeeId;
            
            attendance.CheckOut = currentTime;
            attendance.EarlyLeaveMinutes = GetDuration(ShiftEnd - currentTime);
            attendance.WorkingHours = attendance.CheckOut.HasValue ? Math.Round(
                (decimal)(currentTime - attendance.CheckIn.Value).TotalHours, 2) : null;
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
