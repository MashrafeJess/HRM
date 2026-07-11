using Application.DTOs;
using MediatR;

namespace Application.Features.Employee.GetById;

public record GetEmployeeQuery(long EmployeeId) : IRequest<EmployeeDto>;