using MediatR;

namespace Application.Features.Auth.ChangePassword;

public abstract record ChangePasswordCommand(
    long EmployeeId,        // taken from JWT claim, not from request body
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword) : IRequest<Unit>;