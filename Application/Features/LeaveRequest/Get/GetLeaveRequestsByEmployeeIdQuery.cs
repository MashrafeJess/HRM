using Application.DTOs;
using MediatR;

namespace Application.Features.LeaveRequest.Get;

public record GetEmployeeLeaveRequestsByEmployeeIdQuery(long EmployeeId) : IRequest<List<LeaveRequestDto>>;