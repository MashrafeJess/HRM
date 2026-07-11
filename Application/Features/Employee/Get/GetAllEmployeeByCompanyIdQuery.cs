using Application.DTOs;
using MediatR;

namespace Application.Features.Employee.Get;

public record GetAllEmployeeByCompanyIdQuery(long CompanyId, string ViewOrder, int PageNumber,int PageSize) : IRequest<List<EmployeeDto>>;