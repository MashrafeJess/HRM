using Application.Common.Enums;
using Application.DTOs;
using MediatR;

namespace Application.Features.LeaveRequest.GetByStatus;

public record GetLeaveRequestByStatusQuery(LeaveRequestStatusEnum Id, long CompanyId) : IRequest<List<LeaveRequestDto>>;