using Application.DTOs;
using MediatR;

namespace Application.Features.Attendance.GetAttendanceByDate;

public record  GetAttendanceByDateQuery(long CompanyId, DateOnly Date) : IRequest<List<AttendanceDto>>;