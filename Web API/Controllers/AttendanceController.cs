using Application.Features.Attendance.CheckIn_CheckOut;
using Application.Features.Attendance.GetAttendanceByDate;
using Application.Features.Attendance.GetByEmployeeId;
using Application.Features.Attendance.Summary;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator; 
    
    [HttpPost("CheckIn&CheckOut")]
    public async Task<IActionResult> CheckIn_CheckOut(CheckInCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("GetAttendanceByDate")]
    public async Task<IActionResult> CheckIn([FromQuery] GetAttendanceByDateQuery request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        return Ok(result);
    }
    [HttpGet("GetAttendanceByEmployeeId")]
    public async Task<IActionResult> CheckIn([FromQuery] GetAttendanceByEmployeeIdQuery request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        return Ok(result);
    }
    
    [HttpGet("GetAttendancesStatisticsByEmployeeId")]
    public async Task<IActionResult> CheckIn([FromQuery] GetAttendancesStatisticsByEmployeeId request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        return Ok(result);
    }
    
    
    [HttpGet("GetAttendanceSummaryForMonth")]
    public async Task<IActionResult> CheckIn([FromQuery] GetAttendanceSummaryForMonth request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        return Ok(result);
    }
    
    [HttpGet("GetAttendanceSummaryForADay")]
    public async Task<IActionResult> CheckIn([FromQuery] GetAttendanceSummaryForADay request, CancellationToken ct)
    {
        var result = await _mediator.Send(request, ct);
        return Ok(result);
    }
    
}