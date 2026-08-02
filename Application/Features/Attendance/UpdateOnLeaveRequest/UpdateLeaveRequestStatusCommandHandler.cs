using Application.Common.Constants;
using Application.Interface;
using MediatR;

namespace Application.Features.Attendance.UpdateOnLeaveRequest;

public class UpdateLeaveRequestStatusCommandHandler(IAttendanceRepository repository) : IRequestHandler<UpdateLeaveRequestStatusCommand, bool>
{
    private readonly IAttendanceRepository _repository = repository;
    public async Task<bool> Handle(UpdateLeaveRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var attendanceList = await _repository.GetAttendanceOnCertainRange(request.Dto.EmployeeId, request.FromDate, request.ToDate, cancellationToken);
        
        try
        {
            for (var date = request.FromDate; date <= request.ToDate; date = date.AddDays(1))
            {
                if (WeeklyHolidayCalendar.IsHoliday(date))
                {
                    continue;
                }

                var attendance = (attendanceList.FirstOrDefault(a => a.AttendanceDate == date));

                if (attendance == null)
                {
                    attendance = new Domain.Models.Attendance
                    {
                        CompanyId = request.Dto.CompanyId,
                        EmployeeId = request.Dto.EmployeeId,
                        AttendanceDate = date,
                        Status = "On Leave",
                        Remarks = request.Dto.Remarks,
                        CreatedAt = DateTime.Now
                    };
                }
                else
                {
                    attendance.Status = "On Leave";
                    attendance.Remarks = request.Dto.Remarks;
                }

                await _repository.CreateOrUpdateAttendance(attendance, cancellationToken);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return true;
    }
}
