using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Department.Get;

public class GetAllDepartmentByCompanyIdQueryHandler(IDepartmentRepository repository) : IRequestHandler<GetAllDepartmentByCompanyIdQuery, List<DepartmentDto>>
{
    private readonly IDepartmentRepository _repository = repository;
    public async Task<List<DepartmentDto>> Handle(GetAllDepartmentByCompanyIdQuery request, CancellationToken cancellationToken)
    {
        var departments = await _repository.GetAllDepartments(request.CompanyId, cancellationToken);
        return departments.Select(d=> new DepartmentDto
        {
            DepartmentId = d.DepartmentId,
            CompanyId = d.CompanyId,
            DepartmentName = d.DepartmentName,
            Description = d.Description,
            IsActive = d.IsActive,
        }).ToList();
    }
}