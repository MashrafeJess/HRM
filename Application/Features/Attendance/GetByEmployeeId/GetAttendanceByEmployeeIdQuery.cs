using Application.DTOs;
using MediatR;

namespace Application.Features.Attendance.GetByEmployeeId;

public record  GetAttendanceByEmployeeIdQuery(long EmployeeId): IRequest<List<AttendanceDto>>;