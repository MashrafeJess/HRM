using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Department.Get;

public class GetAllDepartmentByCompanyIdQueryHandler(IDepartmentRepository repository) : IRequestHandler<GetAllDepartmentByCompanyIdQuery, PagedResult<DepartmentDto>>
{
    private readonly IDepartmentRepository _repository = repository;
    public async Task<PagedResult<DepartmentDto>> Handle(GetAllDepartmentByCompanyIdQuery request, CancellationToken cancellationToken)
    {
        var (departments, totalCount) = await _repository.GetAllDepartments(request.CompanyId, request.ViewOrder, request.PageNumber, request.PageSize, cancellationToken);
        return new PagedResult<DepartmentDto>
        {
            Items = departments.Select(d => new DepartmentDto
            {
                DepartmentId = d.Department.DepartmentId,
                CompanyId = d.Department.CompanyId,
                DepartmentName = d.Department.DepartmentName,
                Description = d.Department.Description,
                IsActive = d.Department.IsActive,
                EmployeeCount = d.EmployeeCount,
            }).ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
