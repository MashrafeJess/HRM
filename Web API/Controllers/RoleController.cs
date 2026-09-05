using Application.Features.Role.Get;
using Application.Features.Role.GetById;
using Application.Features.Role.Upsert;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleController(IMediator mediator) : ControllerBase
{
   private readonly IMediator _mediator = mediator;
   [HttpPost("AddOrUpdateRole")]
   [Authorize(Roles = "Super Admin")]
   public async Task<IActionResult> EditRole(EditRoleCommand command, CancellationToken ct)
   {
      var result = await _mediator.Send(command, ct);
      return Ok(result);
   }

   [HttpGet("GetAllRoles")]
   [Authorize(Roles = "Super Admin")]
   public async Task<IActionResult> GetAllRoles(CancellationToken ct)
   {
      var result = await _mediator.Send(new GetRoleQuery(), ct);
      return Ok(result);
   }

   [HttpGet("GetRoleById/{roleId:long}")]
   [Authorize(Roles = "Super Admin")]
   public async Task<IActionResult> GetEmployeeById( long roleId, CancellationToken ct)
   {
      var query = new GetRoleByIdQuery(roleId);
      var result = await _mediator.Send(query, ct);
      return Ok(result);
   }
}