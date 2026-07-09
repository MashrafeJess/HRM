using Application.Common.Exceptions;
using Application.Interface;
using MediatR;

namespace Application.Features.Auth.ChangePassword;

public class ChangePasswordCommandHandler(
    IAuthRepository repo,
    IPasswordService passwordService) : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IPasswordService _passwordService = passwordService;

    public async Task<Unit> Handle(
        ChangePasswordCommand request,
        CancellationToken ct)
    {
        // 1. Validate new password matches confirm
        if (request.NewPassword != request.ConfirmPassword)
            throw new BadRequestException("Passwords do not match.");

        // 2. Get employee from DB
        var employee = await repo.GetEmployeeByIdAsync(request.EmployeeId, ct);
        if (employee == null)
            throw new NotFoundException("Employee not found.");

        // 3. Verify current password is correct
        if (!_passwordService.Verify(employee, request.CurrentPassword))
            throw new BadRequestException("Current password is incorrect.");

        // 4. Make sure new password is different
        if (_passwordService.Verify(employee, employee.PasswordHash))
            throw new BadRequestException("New password must be different from current password.");

        // 5. Hash new password and update
        employee.PasswordHash = _passwordService.Hash(employee, request.NewPassword);
        employee.UpdatedAt = DateTime.UtcNow;

        await repo.UpdateEmployeeAsync(employee, ct);

        // 6. Revoke all refresh tokens → forces re-login on all devices
        await repo.RevokeAllUserTokensAsync(request.EmployeeId, ct);

        await repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}