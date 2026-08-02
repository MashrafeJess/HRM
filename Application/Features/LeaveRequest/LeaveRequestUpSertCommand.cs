using Application.DTOs;
using MediatR;

namespace Application.Features.LeaveRequest;

public record LeaveRequestUpSertCommand(LeaveRequestDto Dto) : IRequest<Unit>;