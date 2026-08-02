using Application.Common.Exceptions;
using Application.DTOs;
using Application.Interface;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AttendanceRepository(IAppDbContext context) : IAttendanceRepository
{
    private readonly IAppDbContext _context = context;

    public async Task<Attendance?> CreateOrUpdateAttendance(Attendance attendance, CancellationToken ct)
    {
        var existingAttendance = await _context.Attendances.FindAsync([attendance.AttendanceId], ct);
        
        if (existingAttendance is null)
        {
            await _context.Attendances.AddAsync(attendance, ct);
        }
        else
        {
            _context.Attendances.Update(attendance);
        }
        await _context.SaveChangesAsync(ct);
        return existingAttendance;
    }

    public async Task<List<Attendance>> GetAttendancesByEmployeeId(long employeeId, CancellationToken ct)
    {
        var attendanceList = await _context.Attendances.Where(a => a.EmployeeId == employeeId).ToListAsync(ct);
        return attendanceList;
    }

    public async Task<List<Attendance>> GetAttendanceByDate(long companyId, DateOnly date, CancellationToken ct)
    {
        var attendanceList = await _context.Attendances.Where(e=>e.CompanyId == companyId && e.AttendanceDate == date).ToListAsync(ct);
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
                (a.LateMinutes ?? null) >  new TimeOnly(0,0)),
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
            
            var totalPresents = attendanceList.Count(a =>
                string.Equals(a.Status, "Present", StringComparison.OrdinalIgnoreCase));
            var totalLate = attendanceList.Count(a =>
                string.Equals(a.Status, "Late", StringComparison.OrdinalIgnoreCase) ||
                (a.LateMinutes.HasValue && a.LateMinutes.Value > TimeOnly.MinValue));
            
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
                                                LateCount = g.Count(x =>
                                                    string.Equals(x.Status, "Late", StringComparison.OrdinalIgnoreCase) ||
                                                    (x.LateMinutes.HasValue && x.LateMinutes.Value > TimeOnly.MinValue)),
                                                LateRate = !g.Any()
                                                    ? 0m
                                                    : Math.Round(g.Count(x =>
                                                        string.Equals(x.Status, "Late", StringComparison.OrdinalIgnoreCase) ||
                                                        (x.LateMinutes.HasValue && x.LateMinutes.Value > TimeOnly.MinValue)) * 100m / g.Count(), 2),
                                                
                                            })
                .OrderBy(e => e.LateRate)
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
            var absenteeEmployee = highestAbsentNigga is null ? null :  employeeList.FirstOrDefault(e => e.EmployeeId == highestAbsentNigga.EmployeeId);

            var result = new AttendanceSummaryDto
            {
                AverageAttendanceRate = averageAttendanceRate,
                TotalLateArrivals = totalLate,
                NumOfPerfectAttendance = employeesWithPerfectAttendance.Count,
                EmployeeList = employeesWithPerfectAttendance,
                MostPunctualDepartmentId = punctualDepartment?.DepartmentId,
                MostPunctualDepartmentName = punctualDepartment?.DepartmentName,
                LateRate = punctualDepartment?.LateRate,
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

    public async Task<AttendanceSummaryForADayDto> GetAttendanceSummaryForADay(long companyId, DateOnly date, CancellationToken ct )
    {
        var attendanceList = await _context.Attendances
            .Where(a => a.CompanyId == companyId && a.AttendanceDate == date)
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

    public async Task<Attendance> GetAttendanceById(long? attendanceId, CancellationToken ct)
    {
        try
        {
            var attendance = await _context.Attendances.Where(a => a.AttendanceId == attendanceId)
                .FirstOrDefaultAsync(ct);
            
            return attendance ?? throw new NotFoundException("This attendance does not exist"); 
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task MarkAbsentEmployeeAsync(DateOnly date, CancellationToken ct)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"EXEC dbo.sp_MarkAbsentEmployees @AttendanceDate = {date}", ct
        );
    }

    public async Task<List<Attendance>> GetAttendanceOnCertainRange(long employeeId, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        try
        {
            var attendanceList = _context.Attendances.Where(a => a.EmployeeId == employeeId && 
                                                                 a.AttendanceDate >= fromDate && a.AttendanceDate <= toDate).ToListAsync(ct);

            return await attendanceList;
        }
        catch (Exception ex)
        {
            throw new Exception("Leave Request Status couldn't be updated", ex);
        }
    }
    
}
