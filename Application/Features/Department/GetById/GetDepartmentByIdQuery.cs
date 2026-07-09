using Application.DTOs;
using MediatR;

namespace Application.Features.Department.GetById;

public record GetDepartmentByIdQuery(long DepartmentId) : IRequest<DepartmentDto>;