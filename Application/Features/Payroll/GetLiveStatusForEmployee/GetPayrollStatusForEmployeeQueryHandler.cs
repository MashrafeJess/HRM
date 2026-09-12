using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Payroll.GetLiveStatusForEmployee;

public class GetPayrollStatusForEmployeeQueryHandler(IPayrollRepository repository)
    : IRequestHandler<GetPayrollStatusForEmployeeQuery, PayrollDto>
{
    private readonly IPayrollRepository _repository = repository;

    public async Task<PayrollDto> Handle(GetPayrollStatusForEmployeeQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;

        return await _repository.GetLivePayrollStatusForEmployee(
            request.EmployeeId, now.Month, now.Year, cancellationToken);
    }
}
