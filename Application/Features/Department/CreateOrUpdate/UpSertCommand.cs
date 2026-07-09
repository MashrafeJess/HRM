using Application.DTOs;
using MediatR;

namespace Application.Features.Department.CreateOrUpdate;

public record UpsertDepartmentCommand(
    DepartmentDto Dto
) :  IRequest<DepartmentDto>;