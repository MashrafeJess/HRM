using Application.DTOs;
using MediatR;

namespace Application.Features.Payroll.GetLiveStatusForEmployee;

public record GetPayrollStatusForEmployeeQuery(long EmployeeId) : IRequest<PayrollDto>;
