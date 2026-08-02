using Application.DTOs;
using MediatR;

namespace Application.Features.Attendance.UpdateOnLeaveRequest;

public record UpdateLeaveRequestStatusCommand(AttendanceDto Dto, DateOnly FromDate, DateOnly ToDate, CancellationToken Ct) : IRequest<bool>;