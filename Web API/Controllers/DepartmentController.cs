using Application.Common.Exceptions;
using Application.DTOs;
using Application.Features.Company.CreateOrUpdate;
using Application.Features.Company.Get;
using Application.Features.Department.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment(CreateOrUpdateCompanyUpSertCommand command, CancellationToken ct = default)
    {
        try
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Exception e)
        {
            throw new BadRequestException("Couldn't Create Company : "+e.Message);

        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartments(GetCompaniesQuery query ,CancellationToken ct = default)
    {
        try
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception e)
        {
            throw new BadRequestException("Couldn't fetch Companies : "+e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartmentById(GetDepartmentByIdQuery query, CancellationToken ct = default)
    {
        try
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (Exception e)
        {
            throw new BadRequestException("Couldn't fetch Companies : "+e.Message);
        }
    }
    

}