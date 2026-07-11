using Application.Features.Company.CreateOrUpdate;
using Application.Features.Company.Get;
using Application.Features.Company.GetById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController(IMediator mediator) : ControllerBase
{
    [HttpPost("EditCompany")]
    public async Task<IActionResult> Handle(CreateOrUpdateCompanyUpSertCommand request,
        CancellationToken cancellationToken)
    {
        var company = await mediator.Send(request, cancellationToken);
        return Ok(company);
    }

    [HttpGet("GetAllCompany")]
    public async Task<IActionResult> GetCompanies([FromQuery] GetCompaniesQuery query, CancellationToken ct)
    {
        var result =  await mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("GetCompanyById/{companyId:long}")]
    public async Task<IActionResult> GetCompanyById(long companyId, CancellationToken ct)
    {
        var query = new GetCompanyByIdQuery(companyId);
        var result = await mediator.Send(query, ct);
        return Ok(result);
    }
}