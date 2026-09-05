using Application.Interface;
using Application.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Features.LeaveRequest.UpSert;

public class LeaveRequestUpSertCommandHandler(
    ILeaveRepository repository,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<LeaveRequestUpSertCommand, Unit>
{
    private readonly ILeaveRepository _repository = repository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    public async Task<Unit> Handle(LeaveRequestUpSertCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (dto.FromDate > dto.ToDate)
        {
            throw new ArgumentException("FromDate must not be later than ToDate.");
        }

        var totalDays = (dto.ToDate - dto.FromDate).Days + 1;
        
        var leave = await _repository.GetLeaveRequestById(request.Dto.LeaveRequestId ?? 0, cancellationToken).ConfigureAwait(false);

        if (leave is null)
        {
            leave = new Domain.Models.LeaveRequest
            {
                CompanyId = dto.CompanyId,
                EmployeeId = dto.EmployeeId,
                LeaveTypeId = dto.LeaveTypeId,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate,
                TotalDays = totalDays,
                Reason = dto.Reason ?? " ",
                Status = dto.Status ?? LeaveRequestStatusEnum.Pending.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            await _repository.ApplyLeave(leave, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            leave.CompanyId = dto.CompanyId;
            leave.EmployeeId = dto.EmployeeId;
            leave.FromDate = dto.FromDate;
            leave.Reason =  dto.Reason ?? " ";
            leave.ToDate = dto.ToDate;
            leave.Ainotes = dto.Ainotes;
            leave.Airecommendation = dto.Airecommendation;
            var status = dto.Status ?? leave.Status;
            leave.Status = status;

            if (string.Equals(status, LeaveRequestStatusEnum.Approved.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var userIdValue = _httpContextAccessor.HttpContext?.User
                    .FindFirstValue("NameIdentifier");

                if (!long.TryParse(userIdValue, out var userId))
                {
                    throw new UnauthorizedAccessException(
                        "The authenticated user's employee ID was not found in the token.");
                }

                leave.ApprovedBy = userId;
                leave.ApprovedAt = DateTime.UtcNow;
            }
            else
            {
                leave.ApprovedBy = null;
                leave.ApprovedAt = null;
            }

            leave.LeaveTypeId = dto.LeaveTypeId;
            leave.TotalDays = totalDays;
            
            await _repository.UpdateLeaveRequest(leave, cancellationToken).ConfigureAwait(false);
        }
        return Unit.Value;
    }
}
