using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.LeaveRequest.Get;

public class GetLeaveRequestsByEmployeeIdQueryHandler(ILeaveRepository repository) : IRequestHandler<GetEmployeeLeaveRequestsByEmployeeIdQuery, List<LeaveRequestDto>>
{
    private readonly ILeaveRepository _repository = repository;
    
    public async Task<List<LeaveRequestDto>> Handle(GetEmployeeLeaveRequestsByEmployeeIdQuery request, CancellationToken cancellationToken)
    {
        var leaves = await _repository.LeaveStatusByEmployeeId(request.EmployeeId, cancellationToken).ConfigureAwait(false);
        return leaves.Select( a=> new LeaveRequestDto
            {
                LeaveRequestId = a.LeaveRequestId,
                CompanyId = a.CompanyId,
                EmployeeId = a.EmployeeId,
                LeaveTypeId = a.LeaveTypeId,
                FromDate = a.FromDate,
                ToDate = a.ToDate,
                TotalDays = a.TotalDays,
                Reason = a.Reason,
                Status = a.Status,
                ApprovedBy = a.ApprovedBy,
                ApprovedAt = a.ApprovedAt,
                Airecommendation = a.Airecommendation,
                Ainotes = a.Ainotes,
            })
        .ToList();
    }
}