using Application.Common;
using Application.DTOs;
using Application.Features.Employee.CreateOrUpdate;
using Application.Features.Employee.Get;
using Application.Features.Employee.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("AddOrUpdateEmployee")]
    [Authorize(Roles = "Company Admin")]
    public async Task<IActionResult> EditEmployee(CreateOrUpdateUpSertCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("GetAllEmployeesByCompanyId/{companyId:long}")]
    [Authorize(Roles = "Company Admin")]
    public async Task<IActionResult> GetAllEmployeeByCompanyId([FromRoute] long companyId, [FromQuery] long? departmentId, [FromQuery] PageFilterDto dto, CancellationToken ct)
    {
            var (pageNumber, pageSize) = PaginationDefaults.Normalize(dto.PageNumber, dto.PageSize);
            var query = new GetAllEmployeeByCompanyIdQuery(companyId, departmentId, dto.ViewOrder??"desc", pageNumber, pageSize);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
    }

    [HttpGet("GetEmployeeById/{employeeId:long}")]
    [Authorize(Roles = "Company Admin")]
    public async Task<IActionResult> GetEmployeeById( long employeeId, CancellationToken ct)
    {
        var query = new GetEmployeeQuery(employeeId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}
