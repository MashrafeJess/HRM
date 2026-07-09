using Application.DTOs;
using MediatR;

namespace Application.Features.Employee.CreateOrUpdate;

public record CreateOrUpdateUpSertCommand(EmployeeDto Dto) : IRequest<EmployeeDto>;