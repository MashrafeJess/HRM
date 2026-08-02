using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Attendance.GetByEmployeeId;

public class GetAttendanceByEmployeeIdQueryHandler(IAttendanceRepository repository) : IRequestHandler<GetAttendanceByEmployeeIdQuery,List<AttendanceDto>>
{
    private readonly IAttendanceRepository _repository = repository;
    public async Task<List<AttendanceDto>> Handle(GetAttendanceByEmployeeIdQuery request, CancellationToken cancellationToken)
    {
        var attendance = await _repository.GetAttendancesByEmployeeId(request.EmployeeId, cancellationToken);
        
        return attendance.Select(a => new AttendanceDto
            {
                AttendanceId = a.AttendanceId,
                CompanyId = a.CompanyId,
                EmployeeId = a.EmployeeId,
                AttendanceDate = a.AttendanceDate,
                CheckIn = a.CheckIn,
                CheckOut = a.CheckOut,
                WorkingHours = a.WorkingHours,
                LateMinutes = a.LateMinutes.HasValue ? (int)a.LateMinutes.Value.ToTimeSpan().TotalMinutes : null, 
                EarlyLeaveMinutes = a.EarlyLeaveMinutes.HasValue ? (int)a.EarlyLeaveMinutes.Value.ToTimeSpan().TotalMinutes : null,
                Status = a.Status,
                Remarks = a.Remarks,
            })
        .OrderBy(a => a.AttendanceId)
        .ToList();
    }
}