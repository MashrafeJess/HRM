using Application.Common.Exceptions;
using Application.DTOs;
using Application.Interface;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Features.LeaveRequest.Get;

public class GetLeaveRequestsByEmployeeIdQueryHandler(ILeaveRepository repository, IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetEmployeeLeaveRequestsByEmployeeIdQuery, List<LeaveRequestDto>>
{
    private readonly ILeaveRepository _repository = repository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<List<LeaveRequestDto>> Handle(GetEmployeeLeaveRequestsByEmployeeIdQuery request, CancellationToken cancellationToken)
    {
        // A "Common" employee may only view their own leave history; a "Company Admin"
        // may view any employee's. [Authorize(Roles = "Company Admin,Common")] on the
        // controller only checks role membership — it can't express "and only their own
        // record" — so that part is enforced here using the NameIdentifier claim.
        var user = _httpContextAccessor.HttpContext?.User;
        var role = user?.FindFirstValue("Role") ?? "";
        if (role != "Company Admin")
        {
            var callerEmployeeIdRaw = user?.FindFirstValue("NameIdentifier");
            if (!long.TryParse(callerEmployeeIdRaw, out var callerEmployeeId) || callerEmployeeId != request.EmployeeId)
            {
                throw new ForbiddenException("You can only view your own leave requests.");
            }
        }

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