using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Employee.Get;

public class GetAllEmployeeByCompanyIdQueryHandler(IEmployeeRepository repository) : IRequestHandler<GetAllEmployeeByCompanyIdQuery, PagedResult<EmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository = repository;
    public async Task<PagedResult<EmployeeDto>> Handle(GetAllEmployeeByCompanyIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _employeeRepository
            .GetAllEmployeesByCompanyId(request.CompanyId, request.DepartmentId, request.ViewOrder,request.PageNumber, request.PageSize, cancellationToken);
        return result;
    }
}
