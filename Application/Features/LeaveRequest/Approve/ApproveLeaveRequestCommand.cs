using MediatR;

namespace Application.Features.LeaveRequest.Approve;

public record ApproveLeaveRequestCommand(long LeaveRequestId) : IRequest<Unit>;
