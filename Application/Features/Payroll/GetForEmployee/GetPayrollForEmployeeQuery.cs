using Application.DTOs;
using MediatR;

namespace Application.Features.Payroll.GetForEmployee;

public record GetPayrollForEmployeeQuery(long EmployeeId, long MonthId, long YearId, CancellationToken Ct) : IRequest<PayrollDto>;