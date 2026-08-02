using Application.Common.Exceptions;
using Application.Common.Enums;
using Application.Interface;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class LeaveRepository(IAppDbContext context)  : ILeaveRepository
{
    private readonly IAppDbContext _context = context;

    public async Task<LeaveRequest> ApplyLeave(LeaveRequest leaveRequest, CancellationToken cancellationToken)
    {
        try
        {
           var result = await _context.LeaveRequests.AddAsync(leaveRequest, cancellationToken);
           await _context.SaveChangesAsync(cancellationToken);
           return result.Entity;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<LeaveRequest> UpdateLeaveRequest(LeaveRequest leaveRequest, CancellationToken cancellationToken)
    {
        try
        {
            var entry = await _context.LeaveRequests.FindAsync([leaveRequest.LeaveRequestId, cancellationToken], cancellationToken);
            if (entry != null)
            {
                _context.LeaveRequests.Update(leaveRequest);
                await _context.SaveChangesAsync(cancellationToken);
                return leaveRequest;
            }
            else
            {
                throw new NotFoundException("This  leave request was not found.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<List<LeaveRequest>> LeaveStatusByEmployeeId(long employeeId, CancellationToken ct)
    {
        try
        {
            var leaveRequests = await _context.LeaveRequests.Where(e => e.EmployeeId == employeeId).ToListAsync(ct);
            return leaveRequests;
        }
        catch (Exception ex)
        {
            throw new Exception("Couldn't fetch Leave Requests", ex);
        }
    }

    public async Task<List<LeaveRequest>> LeaveRequestsByStatus(LeaveRequestStatusEnum statusId, long companyId, CancellationToken ct)
    {
        try
        {
            var query = _context.LeaveRequests.Where(l => l.CompanyId == companyId).AsQueryable();

            var leaveRequests = from l in query
                join approver in _context.Employees
                    on l.ApprovedBy equals (long?)approver.EmployeeId into approverJoin
                from approver in approverJoin.Where(e => e.IsActive).DefaultIfEmpty()
                select new
                {
                    LeaveRequest = l,
                    ApprovedByName = approver == null
                        ? null
                        : approver.FirstName + " " + approver.LastName
                };

            if (statusId != LeaveRequestStatusEnum.All)
            {
                var status = statusId.ToString();
                leaveRequests = leaveRequests.Where(l => l.LeaveRequest.Status == status);
            }

            var results = await leaveRequests.ToListAsync(ct);
            foreach (var result in results)
            {
                result.LeaveRequest.ApprovedByName = result.ApprovedByName;
            }

            return [.. results.Select(result => result.LeaveRequest)];
        }
        catch (Exception ex)
        {
            throw new Exception("Leave Request couldn't be fetched", ex);
        }
    }

    public async Task<List<LeaveRequest>> LeaveHistoryByEmployeeId(long employeeId, CancellationToken ct)
    {
        try
        {
            var leaveRequests = await _context.LeaveRequests.Where(l => l.EmployeeId == employeeId).ToListAsync(ct);
            return leaveRequests;
        }
        catch (Exception ex)
        {
            throw new Exception("Couldn't fetch Leave History", ex);
        }
    }

    public async Task<LeaveRequest?> GetLeaveRequestById(long leaveId, CancellationToken ct)
    {
        try
        {
            var leaveRequest = await _context.LeaveRequests.FindAsync([leaveId, ct], ct);
            return leaveRequest;
        }
        catch (Exception ex)
        {
            throw new Exception("The issue is : " + ex.Message, ex);
        }
    }
}
