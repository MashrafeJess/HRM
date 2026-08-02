using Application.Common.Enums;
using Domain.Models;

namespace Application.Interface;

public interface ILeaveRepository
{
    public Task<LeaveRequest> ApplyLeave(LeaveRequest leaveRequest, CancellationToken cancellationToken);

    public Task<LeaveRequest> UpdateLeaveRequest(LeaveRequest leaveRequest, CancellationToken cancellationToken);

    public Task<List<LeaveRequest>> LeaveStatusByEmployeeId(long employeeId, CancellationToken ct); //Done

    public Task<List<LeaveRequest>> LeaveRequestsByStatus(LeaveRequestStatusEnum statusId, long companyId,
        CancellationToken ct);

    public Task<List<LeaveRequest>> LeaveHistoryByEmployeeId(long employeeId, CancellationToken ct);

    public Task<LeaveRequest?> GetLeaveRequestById(long leaveId, CancellationToken ct);

    //Task<object> LeaveRequestsByStatus(LeaveRequestStatusEnum statusId, CancellationToken cancellationToken);
}