using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.LeaveRequest.GetHistory;

public class GetLeaveHistoryByEmployeeQueryHandler(ILeaveRepository repository) : IRequestHandler<GetLeaveRequestByEmployeeIdQuery, List<LeaveRequestDto>>
{
    private readonly ILeaveRepository _repository = repository;
    public async Task<List<LeaveRequestDto>> Handle(GetLeaveRequestByEmployeeIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.LeaveHistoryByEmployeeId(request.EmployeeId, cancellationToken).ConfigureAwait((false));
        return result.Select(x => new LeaveRequestDto
            {
                LeaveRequestId = x.LeaveRequestId,
                CompanyId = x.CompanyId,
                EmployeeId = 0,
                LeaveTypeId = x.LeaveTypeId,
                FromDate = x.FromDate,
                ToDate = x.ToDate,
                TotalDays = x.TotalDays,
                Reason = x.Reason,
                Status = x.Status,
                ApprovedBy = x.ApprovedBy,
                ApprovedAt = x.ApprovedAt
            })
        .ToList();
    }
}