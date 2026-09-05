using Application.Common;
using Application.Common.Exceptions;
using Application.DTOs;
using Application.Features.Company.CreateOrUpdate;
using Application.Features.Company.Get;
using Application.Features.Department.CreateOrUpdate;
using Application.Features.Department.Get;
using Application.Features.Department.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost("EditDepartment")]
    [Authorize(Roles = "Company Admin")]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment(UpsertDepartmentCommand command, CancellationToken ct = default)
    {
        try
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Exception e)
        {
            throw new BadRequestException("Couldn't Create Department : "+e.Message);

        }
    }

    [HttpGet("AllDepartmentsByCompanyId/{companyId:long}")]
    [Authorize(Roles = "Company Admin")]
    public async Task<IActionResult> GetDepartments([FromRoute] long companyId, [FromQuery] PageFilterDto dto, CancellationToken ct = default)
    {
        try
        {
            var (pageNumber, pageSize) = PaginationDefaults.Normalize(dto.PageNumber, dto.PageSize);
            var query = new GetAllDepartmentByCompanyIdQuery(companyId, dto.ViewOrder ?? "desc", pageNumber, pageSize);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception e)
        {
            throw new BadRequestException("Couldn't fetch Departments : "+e.Message);
        }
    }

    [HttpGet("GetDepartmentById{departmentId:long}")]
    [Authorize(Roles = "Company Admin")]
    public async Task<IActionResult> GetDepartmentById(long departmentId, CancellationToken ct = default)
    {
        try
        {
            var query = new GetDepartmentByIdQuery(departmentId);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception e)
        {
            throw new BadRequestException("Couldn't fetch Companies : "+e.Message);
        }
    }
}