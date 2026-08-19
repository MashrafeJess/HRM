using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Payroll.GetForEmployee;

public class GetPayrollForEmployeeQueryHandler(IPayrollRepository repository) : IRequestHandler<GetPayrollForEmployeeQuery, PayrollDto>
{
    private readonly IPayrollRepository _repository = repository;

    public async Task<PayrollDto> Handle(GetPayrollForEmployeeQuery request, CancellationToken cancellationToken)
    {
        var payroll =
            await _repository.GetPayrollForEmployee(request.EmployeeId, request.YearId, request.MonthId,
                cancellationToken).ConfigureAwait(false);
        
        return new PayrollDto
        {
            PayrollId = payroll.PayrollId,
            CompanyId = payroll.CompanyId,
            EmployeeId = payroll.EmployeeId,
            Month = payroll.Month,
            Year = payroll.Year,
            BasicSalary = payroll.BasicSalary,
            AbsentDeduction = payroll.AbsentDeduction,
            LateDeduction = payroll.LateDeduction,
            NetSalary = payroll.NetSalary,
        };
    }
}