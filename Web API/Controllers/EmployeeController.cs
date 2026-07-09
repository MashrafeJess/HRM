using Application.DTOs;
using Application.Features.Employee.CreateOrUpdate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> EditEmployee([FromBody] EmployeeDto dto, CancellationToken ct)
    {
        var command = new CreateOrUpdateUpSertCommand(dto);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}
