using Application.Features.Payroll.GetForCompany;
using Application.Features.Payroll.GetForEmployee;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayrollController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    [HttpGet("GetPayRollForEmployee")]
    public async Task<IActionResult> GetPayRollForEmployee([FromQuery]long employeeId, long yearId, long monthId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayrollForEmployeeQuery(employeeId, monthId, yearId, ct), ct);
        return Ok(result);
    }
    
    [HttpGet("GetPayRollForCompany")]
    public async Task<IActionResult> GetPayRollForCompany([FromQuery]long companyId, long yearId, long monthId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayrollForCompanyQuery(companyId, monthId, yearId, ct), ct);
        return Ok(result);
    }
    
}