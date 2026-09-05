using Application.DTOs;
using MediatR;

namespace Application.Features.Department.Get;

public record GetAllDepartmentByCompanyIdQuery(long CompanyId, string? ViewOrder, int PageNumber, int PageSize) : IRequest<PagedResult<DepartmentDto>>;
