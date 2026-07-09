using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Department.CreateOrUpdate;

public class UpsertCommandHandler(IDepartmentRepository departmentRepository) : IRequestHandler<UpsertDepartmentCommand, DepartmentDto>
{
    private readonly IDepartmentRepository _departmentRepository = departmentRepository;
    
    public async Task<DepartmentDto> Handle(UpsertDepartmentCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        Domain.Models.Department department;

        if (dto.DepartmentId is null or 0)
        {
            department = new Domain.Models.Department
            {
                CompanyId = dto.CompanyId,
                DepartmentName = dto.DepartmentName,
                Description = dto.Description,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
        }
        else
        {
            var (existingDepartment, _) = await _departmentRepository.GetByDepartmentId(
                dto.DepartmentId.Value, cancellationToken);

            existingDepartment.CompanyId = dto.CompanyId;
            existingDepartment.DepartmentName = dto.DepartmentName;
            existingDepartment.Description = dto.Description;
            existingDepartment.IsActive = dto.IsActive;
            existingDepartment.UpdatedAt = DateTime.UtcNow;

            department = existingDepartment;
        }

        await _departmentRepository.UpSertDepartment(department, cancellationToken);

        return new DepartmentDto
        {
            CompanyId = department.CompanyId,
            DepartmentName = department.DepartmentName,
            Description = department.Description,
            IsActive = department.IsActive,
        };
    }
    
}