using Application.Common;
using Application.DTOs;
using Application.Features.Company.CreateOrUpdate;
using Application.Features.Company.Get;
using Application.Features.Company.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController(IMediator mediator) : ControllerBase
{
    [HttpPost("EditCompany")]
    [Authorize(Roles = "Super Admin, Company Admin")]
    public async Task<IActionResult> Handle(CreateOrUpdateCompanyUpSertCommand request,
        CancellationToken cancellationToken)
    {
        var company = await mediator.Send(request, cancellationToken);
        return Ok(company);
    }

    [HttpGet("GetAllCompany")]
    [Authorize(Roles = "Super Admin")]
    public async Task<IActionResult> GetCompanies([FromQuery] PageFilterDto dto, CancellationToken ct)
    {
        var (pageNumber, pageSize) = PaginationDefaults.Normalize(dto.PageNumber, dto.PageSize);
        var query = new GetCompaniesQuery(dto.ViewOrder, pageNumber, pageSize);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("GetCompanyById/{companyId:long}")]
    [Authorize(Roles = "Super Admin")]
    public async Task<IActionResult> GetCompanyById(long companyId, CancellationToken ct)
    {
        var query = new GetCompanyByIdQuery(companyId);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }
}