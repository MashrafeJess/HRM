using Application.Common.Exceptions;
using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Department.GetById;

public class GetDepartmentByIdQueryHandler(IDepartmentRepository repository) : IRequestHandler<GetDepartmentByIdQuery,DepartmentDto>
{
    private readonly IDepartmentRepository _repository = repository;
    public async Task<DepartmentDto> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var (department, employeeCount) = await _repository.GetByDepartmentId(request.DepartmentId, cancellationToken);
        if (department == null)
        {
            throw new NotFoundException("Department not found");    
        }

        return new DepartmentDto
        {
            DepartmentId =  department.DepartmentId,
            CompanyId = department.CompanyId,
            DepartmentName = department.DepartmentName,
            Description = department.Description,
            IsActive = department.IsActive,
            EmployeeCount = employeeCount,
            CreatedAt =  department.CreatedAt,
            UpdatedAt = department.UpdatedAt
        };
    }
}