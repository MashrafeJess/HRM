using Application.DTOs;
using MediatR;

namespace Application.Features.LeaveRequest.GetHistory;

public record GetLeaveRequestByEmployeeIdQuery(long EmployeeId, CancellationToken Ct) : IRequest<List<LeaveRequestDto>>;