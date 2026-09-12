using Application.Interface;
using Application.Common.Enums;
using MediatR;

namespace Application.Features.LeaveRequest.UpSert;

public class LeaveRequestUpSertCommandHandler(
    ILeaveRepository repository) : IRequestHandler<LeaveRequestUpSertCommand, Unit>
{
    private readonly ILeaveRepository _repository = repository;
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
                Status = LeaveRequestStatusEnum.Pending.ToString(),
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
            leave.LeaveTypeId = dto.LeaveTypeId;
            leave.TotalDays = totalDays;
            
            await _repository.UpdateLeaveRequest(leave, cancellationToken).ConfigureAwait(false);
        }
        return Unit.Value;
    }
}
