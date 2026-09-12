using Application.Common.Exceptions;
using Application.DTOs;
using Application.Interface;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Features.Payroll.GetForEmployee;

public class GetPayrollForEmployeeQueryHandler(IPayrollRepository repository, IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetPayrollForEmployeeQuery, PayrollDto>
{
    private readonly IPayrollRepository _repository = repository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<PayrollDto> Handle(GetPayrollForEmployeeQuery request, CancellationToken cancellationToken)
    {
        // A "Common" employee may only view their own payroll; a "Company Admin" may
        // view any employee's. [Authorize(Roles = "Company Admin,Common")] on the
        // controller only checks role membership — it can't express "and only their
        // own record" — so that part is enforced here using the NameIdentifier claim.
        var user = _httpContextAccessor.HttpContext?.User;
        var role = user?.FindFirstValue("Role") ?? "";
        if (role != "Company Admin")
        {
            var callerEmployeeIdRaw = user?.FindFirstValue("NameIdentifier");
            if (!long.TryParse(callerEmployeeIdRaw, out var callerEmployeeId) || callerEmployeeId != request.EmployeeId)
            {
                throw new ForbiddenException("You can only view your own payroll.");
            }
        }

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