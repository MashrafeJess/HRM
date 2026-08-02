using Application.DTOs;
using MediatR;

namespace Application.Features.Attendance.CheckIn_CheckOut;

public record CheckInCommand(AttendanceDto Dto) : IRequest<AttendanceDto>;