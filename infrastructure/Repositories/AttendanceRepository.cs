using Application.DTOs;
using Application.Interface;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AttendanceRepository(IAppDbContext context)
{
    private readonly IAppDbContext _context = context;

    public async Task<Attendance?> CreateOrUpdateAttendance(Attendance attendance, CancellationToken ct)
    {
        var existingAttendance = await _context.Attendances.FindAsync([attendance.AttendanceId, ct], ct);
        
        if (existingAttendance is null)
        {
            await _context.Attendances.AddAsync(attendance, ct);
        }
        else
        {
            _context.Attendances.Update(attendance);
        }
        return existingAttendance;
    }

    public async Task<List<Attendance>> GetAttendancesByEmployeeId(long employeeId, CancellationToken ct)
    {
        var attendanceList = await _context.Attendances.Where(a => a.EmployeeId == employeeId).ToListAsync(ct);
        return attendanceList;
    }

    public async Task<List<Attendance>> GetAttendanceByDate(DateOnly date, CancellationToken ct)
    {
        var attendanceList = await _context.Attendances.Where(e=>e.AttendanceDate == date).ToListAsync(ct);
        return attendanceList;
    }
    public async Task<AttendanceStatisticsDto> GetAttendancesStatisticsByEmployeeId(long employeeId,long monthId, long yearId, CancellationToken ct)
    {
        var attendanceList = await _context.Attendances
            .Where(e =>
                e.EmployeeId == employeeId &&
                e.AttendanceDate.Month == monthId &&
                e.AttendanceDate.Year == yearId)
            .ToListAsync(ct);

        var stats = new AttendanceStatisticsDto
        {
            PresentDays = attendanceList.Count(a =>
                string.Equals(a.Status, "Present", StringComparison.OrdinalIgnoreCase)),
            LateDays = attendanceList.Count(a =>
                string.Equals(a.Status, "Late", StringComparison.OrdinalIgnoreCase) ||
                (a.LateMinutes ?? 0) > 0),
            LeaveDays = attendanceList.Count(a =>
                string.Equals(a.Status, "Leave", StringComparison.OrdinalIgnoreCase)),
            AttendanceRatio = 0
        };

        var totalAttendanceDays = attendanceList.Count;
        stats.AttendanceRatio = totalAttendanceDays == 0
            ? 0
            : (long)Math.Round((double)(stats.PresentDays * 100) / totalAttendanceDays);

        return stats;
    }

    public async Task<AttendanceSummaryDto> GetAttendanceSummaryForMonth(long companyId, long monthId, long yearId, CancellationToken ct)
    {
        try
        {
            var attendanceList = await _context.Attendances.Where(a=> a.CompanyId == companyId && 
                                                                      a.AttendanceDate.Year == yearId && a.AttendanceDate.Month == monthId).ToListAsync(ct);
            var totalDays = attendanceList.Count;
            
            var totalPresents = attendanceList.Count(a =>a.Status == "Present"); 
            var totalLate = attendanceList.Count(a =>a.Status == "Late");
            
            var averageAttendanceRate = totalDays == 0 ? 0 :  (long)Math.Round((double)(totalPresents * 100) / totalDays);
            
            var employeeList = await _context.Employees.Where(e => e.CompanyId == companyId)
                .Include(e => e.Department)
                .ToListAsync(ct);
            
            var employeesWithPerfectAttendance = (from a in attendanceList
                join e in employeeList on a.EmployeeId equals e.EmployeeId
                where e.IsActive
                   group a by new
            {
                e.EmployeeId,
                e.Department?.DepartmentName,
                e.FirstName,
                e.LastName
            }
            into g
                    select new PerfectAttendanceEmployeeSummaryDto()
                {
                    EmployeeId = g.Key.EmployeeId,
                    EmployeeName = g.Key.FirstName + " " + g.Key.LastName,
                    DepartmentName = g.Key.DepartmentName,
                    TotalAbsent = g.Count(x=>x.Status == "Absent"), 
                })
                .ToList();
                    
            var departmentPunctuality = (from  a in attendanceList
                                        join e in employeeList on a.EmployeeId equals e.EmployeeId
                                        where e.Department != null && e.IsActive
                                            group a by new {e.DepartmentId, e.Department!.DepartmentName } into g
                                            select new
                                            {
                                                g.Key.DepartmentId,
                                                g.Key.DepartmentName,
                                                TotalRecords = g.Count(),
                                                lateCount = g.Count(x=>x.Status == "Late"),
                                                LateRate = g.Average(x=>x.LateMinutes),
                                            })
                .OrderByDescending(e => e.LateRate)
                .ToList();
            
            var punctualDepartment = departmentPunctuality.FirstOrDefault();

            var highestAbsentNigga = attendanceList.Where(a => a.Status == "Absent")
                .GroupBy(a => a.EmployeeId)
                .Select(g => new
                {
                    EmployeeId = g.Key,
                    AbsentCount = g.Count(),
                })
                .OrderByDescending(g => g.AbsentCount)
                .FirstOrDefault();
            var absenteeEmployee = employeeList.FirstOrDefault(e => e.EmployeeId == highestAbsentNigga!.EmployeeId);

            var result = new AttendanceSummaryDto
            {
                AverageAttendanceRate = averageAttendanceRate,
                TotalLateArrivals = totalLate,
                NumOfPerfectAttendance = employeesWithPerfectAttendance.Count,
                EmployeeList = employeesWithPerfectAttendance,
                MostPunctualDepartmentId = punctualDepartment?.DepartmentId,
                MostPunctualDepartmentName = punctualDepartment?.DepartmentName,
                HighestAbsenteeId = highestAbsentNigga?.EmployeeId,
                HighestAbsenteeName = absenteeEmployee?.FirstName,
            };
            return result;
        }
        catch (Exception ex)
        {
           throw new Exception(ex.Message); 
        }
    }

    public async Task<AttendanceSummaryForADayDto> GetAttendanceSummaryForADay(long companyId, long dayId, CancellationToken ct )
    {
        var attendanceList = await _context.Attendances
            .Where(a => a.CompanyId == companyId && a.AttendanceDate.Day == dayId)
            .ToListAsync(ct);

        var totalEmployees = await _context.Employees
            .CountAsync(e => e.CompanyId == companyId && e.IsActive, ct);

        var totalPresent = attendanceList.Count(a =>
            string.Equals(a.Status, "Present", StringComparison.OrdinalIgnoreCase));

        var totalLate = attendanceList.Count(a =>
            string.Equals(a.Status, "Late", StringComparison.OrdinalIgnoreCase));

        var totalLeave = attendanceList.Count(a =>
            string.Equals(a.Status, "Leave", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(a.Status, "On Leave", StringComparison.OrdinalIgnoreCase));

        var totalAbsent = attendanceList.Count(a =>
            string.Equals(a.Status, "Absent", StringComparison.OrdinalIgnoreCase));

        var totalAbsentArrival = attendanceList.Count(a =>
            string.Equals(a.Status, "Absent Arrival", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(a.Status, "AbsentArrival", StringComparison.OrdinalIgnoreCase));

        return new AttendanceSummaryForADayDto
        {
            TotalEmployees = totalEmployees,
            TotalPresent = totalPresent,
            TotalLate = totalLate,
            TotalLeave = totalLeave,
            TotalAbsent = totalAbsent,
            TotalAbsentArrival = totalAbsentArrival
        };
    }
    
}
