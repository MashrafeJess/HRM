using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Employee.Get;

public class GetAllEmployeeByCompanyIdQueryHandler(IEmployeeRepository repository) : IRequestHandler<GetAllEmployeeByCompanyIdQuery, List<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository = repository;
    public async Task<List<EmployeeDto>> Handle(GetAllEmployeeByCompanyIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _employeeRepository
            .GetAllEmployeesByCompanyId(request.CompanyId, request.ViewOrder,request.PageNumber, request.PageSize, cancellationToken);
        return result;
    }
}