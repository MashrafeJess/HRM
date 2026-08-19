using Application.DTOs;
using MediatR;

namespace Application.Features.Payroll.GetForCompany;

public record GetPayrollForCompanyQuery(long CompanyId, long Month, long Year, CancellationToken CancellationToken) : IRequest<List<PayrollDto>>;