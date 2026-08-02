using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.LeaveRequest.GetByStatus;

public class GetLeaveRequestByStatusQueryHandler (ILeaveRepository repository) : IRequestHandler<GetLeaveRequestByStatusQuery, List<LeaveRequestDto>>
{
    private readonly ILeaveRepository _repository = repository;
    public async Task<List<LeaveRequestDto>> Handle(GetLeaveRequestByStatusQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.LeaveRequestsByStatus(request.Id, request.CompanyId, cancellationToken).ConfigureAwait(false);
        return
        [
            .. result.Select(g => new LeaveRequestDto
            {
                LeaveRequestId = g.LeaveRequestId,
                CompanyId = g.CompanyId,
                EmployeeId = g.EmployeeId,
                LeaveTypeId = g.LeaveTypeId,
                FromDate = g.FromDate,
                ToDate = g.ToDate,
                TotalDays = g.TotalDays,
                Reason = g.Reason,
                Status = g.Status,
                ApprovedBy = g.ApprovedBy,
                ApprovedByName = g.ApprovedByName,
                ApprovedAt = g.ApprovedAt,
            })
        ];
    }
}
