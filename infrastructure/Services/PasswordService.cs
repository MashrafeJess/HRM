using Application.Interface;
using Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string Hash(Employee employee, string newPassword)
    {
        var hashedPassword = new PasswordHasher<Employee>().HashPassword(employee, newPassword);
        return hashedPassword;
    }

    public bool Verify(Employee employee, string newPassword)
    {
        var x =  new  PasswordHasher<Employee>().VerifyHashedPassword(employee,employee.PasswordHash, newPassword);
        return x == PasswordVerificationResult.Success;
    }
}