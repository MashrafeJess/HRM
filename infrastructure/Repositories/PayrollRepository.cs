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

    public async Task<List<Payroll>> GetPayrollsForCompany(long companyId, long monthId, long yearId, CancellationToken cancellationToken)
    {
        var payrolls = await _context.Payrolls
            .Where(x => x.CompanyId == companyId && x.Month == monthId && x.Year == yearId)
            .ToListAsync(cancellationToken);
        return payrolls;
    }
}