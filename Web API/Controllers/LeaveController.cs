using Application.Features.LeaveRequest;
using Application.Features.LeaveRequest.Get;
using Application.Features.LeaveRequest.GetByStatus;
using Application.Features.LeaveRequest.GetHistory;
using Application.Features.Attendance.UpdateOnLeaveRequest;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    [HttpPost("AddLeaveRequest")]
    public async Task<IActionResult> AddLeaveRequest(LeaveRequestUpSertCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("GetLeaveRequestByEmployeeId")]
    public async Task<IActionResult> GetLeaveRequestByEmployeeId([FromQuery] long employeeId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLeaveRequestByEmployeeIdQuery(employeeId, ct), ct);
        return Ok(result);
    }

    [HttpGet("GetLeaveRequestByStatus")]
    public async Task<IActionResult> GetLeaveRequestByStatus([FromQuery] GetLeaveRequestByStatusQuery request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        return Ok(result);
    }
    
    [HttpGet("GetEmployeeLeaveRequestsByEmployeeId")]
    public async Task<IActionResult> GetEmployeeLeaveRequestsByEmployeeId([FromQuery] GetEmployeeLeaveRequestsByEmployeeIdQuery request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        return Ok(result);
    }

    [HttpPost("UpdateLeaveRequestStatus")]
    public async Task<IActionResult> UpdateLeaveRequestStatus(
        [FromBody] UpdateLeaveRequestStatusCommand command,
        CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
    
}
