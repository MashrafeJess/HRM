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
                DepartmentId = d.DepartmentId,
                CompanyId = d.CompanyId,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                IsActive = d.IsActive,
            }).ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
