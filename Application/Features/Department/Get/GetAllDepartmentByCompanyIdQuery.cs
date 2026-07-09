using Application.DTOs;
using MediatR;

namespace Application.Features.Department.Get;

public record GetAllDepartmentByCompanyIdQuery(long CompanyId) : IRequest<List<DepartmentDto>>;