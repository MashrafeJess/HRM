using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Payroll.GetForCompany;

public class GetPayrollForCompanyQueryHandler(IPayrollRepository repository) : IRequestHandler<GetPayrollForCompanyQuery, List<PayrollDto>>
{
    private readonly IPayrollRepository _repository = repository;
    public async Task<List<PayrollDto>> Handle(GetPayrollForCompanyQuery request, CancellationToken cancellationToken)
    {
        var payrollList =
            await _repository.GetPayrollsForCompany(request.CompanyId, request.Month, request.Year, cancellationToken);
        
        return
        [
            .. payrollList.Select(x => new PayrollDto
            {
                PayrollId = x.PayrollId,
                CompanyId = x.CompanyId,
                EmployeeId = x.EmployeeId,
                Month = x.Month,
                Year = x.Year,
                BasicSalary = x.BasicSalary,
                AbsentDeduction = x.AbsentDeduction,
                LateDeduction = x.LateDeduction,
                NetSalary = x.NetSalary,
            })
        ];
    }
}