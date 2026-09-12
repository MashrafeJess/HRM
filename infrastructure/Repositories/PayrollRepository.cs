using Application.Common.Exceptions;
using Application.DTOs;
using Application.Interface;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PayrollRepository(IAppDbContext context, IPayrollService service) : IPayrollRepository
{
    private readonly IAppDbContext _context = context;
    private readonly IPayrollService _payrollService = service;
    
    public async Task<bool> HasPayrollBeenGeneratedAsync(long monthId, long yearId, CancellationToken cancellationToken)
    {
        return await _context.Payrolls.AnyAsync(p => p.Month == monthId && p.Year == yearId, cancellationToken);
    }

    public async Task<List<Payroll>> GeneratePayrollsAsync(long monthId, long yearId, CancellationToken cancellationToken)
    {
        try
        {

            var employees = await (from emp in _context.Employees
                    join a in _context.Attendances on emp.EmployeeId equals a.EmployeeId into gg
                    from g in gg.Where(x => x.AttendanceDate.Year == yearId && x.AttendanceDate.Month == monthId)
                        .DefaultIfEmpty()
                    where emp.IsActive
                    group g by new { emp.EmployeeId, emp.CompanyId, emp.Salary } into grouped
                    select new
                    {
                        grouped.Key.EmployeeId,
                        grouped.Key.CompanyId,
                        grouped.Key.Salary,
                        AbsentCount = grouped.Count(x => x != null && x.Status == "Absent"),
                        LateCount = grouped.Count(x => x != null && x.Status == "Late")
                    }
                ).ToListAsync(cancellationToken);

            var payrolls = employees.Select(emp =>
            {
                
                var netSalary = _payrollService.CalculatePayroll(emp.Salary, emp.LateCount, emp.AbsentCount);

                return new Payroll
                { 
                    CompanyId = emp.CompanyId,
                    EmployeeId = emp.EmployeeId,
                    Month = monthId,
                    Year = yearId,
                    BasicSalary = emp.Salary,
                    AbsentDeduction = netSalary.AbsentPenalty,
                    LateDeduction = netSalary.LatePenalty,
                    NetSalary = netSalary.Salary,
                    GeneratedAt = DateTime.Now
                };
            }).ToList();

            await _context.Payrolls.AddRangeAsync(payrolls, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return payrolls;
        }
        catch (Exception ex)
        {
            throw new Exception("The issue is " + ex.Message);
        }
    }

    public async Task<Payroll> GetPayrollForEmployee(long employeeId,long yearId, long monthId, CancellationToken cancellationToken)
    {
        var payroll = await _context.Payrolls
            .Where(x => x.EmployeeId == employeeId && x.Month == monthId && x.Year == yearId).FirstOrDefaultAsync(cancellationToken);
        
        return payroll ?? throw new Exception("Payroll not found");
    }

    public async Task<PayrollDto> GetLivePayrollStatusForEmployee(long employeeId, long monthId, long yearId, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.IsActive, cancellationToken);

        if (employee is null)
        {
            throw new NotFoundException("Employee not found");
        }

        var month = (int)monthId;
        var year = (int)yearId;

        var absentCount = await _context.Attendances.CountAsync(
            a => a.EmployeeId == employeeId && a.AttendanceDate.Year == year && a.AttendanceDate.Month == month
                 && a.Status == "Absent", cancellationToken);

        var lateCount = await _context.Attendances.CountAsync(
            a => a.EmployeeId == employeeId && a.AttendanceDate.Year == year && a.AttendanceDate.Month == month
                 && a.Status == "Late", cancellationToken);

        var calculation = _payrollService.CalculatePayroll(employee.Salary, lateCount, absentCount);

        return new PayrollDto
        {
            // PayrollId stays 0 and GeneratedAt stays null — this is a live estimate,
            // never persisted to the Payroll table.
            CompanyId = employee.CompanyId,
            EmployeeId = employee.EmployeeId,
            EmployeeName = employee.FirstName + " " + employee.LastName,
            Month = monthId,
            Year = yearId,
            BasicSalary = employee.Salary,
            AbsentDeduction = calculation.AbsentPenalty,
            LateDeduction = calculation.LatePenalty,
            NetSalary = calculation.Salary,
        };
    }

    public async Task<List<Payroll>> GetPayrollsForCompany(long companyId, long monthId, long yearId, CancellationToken cancellationToken)
    {
        var payrolls = await (from payroll in _context.Payrolls
            join employee in _context.Employees on payroll.EmployeeId equals employee.EmployeeId
            where payroll.CompanyId == companyId && payroll.Month == monthId && payroll.Year == yearId
            select new Payroll
            {
                PayrollId = payroll.PayrollId,
                CompanyId = payroll.CompanyId,
                EmployeeId = payroll.EmployeeId,
                EmployeeName = employee.FirstName + " " + employee.LastName,
                Month = payroll.Month,
                Year = payroll.Year,
                BasicSalary = payroll.BasicSalary,
                AbsentDeduction = payroll.AbsentDeduction,
                LateDeduction = payroll.LateDeduction,
                NetSalary = payroll.NetSalary,
                GeneratedAt = payroll.GeneratedAt
            })
            .ToListAsync(cancellationToken);
        return payrolls;
    }
}
