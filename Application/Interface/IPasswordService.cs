using Domain.Models;

namespace Application.Interface;

public interface IPasswordService
{
    string Hash(Employee employee, string newPassword);
    bool Verify(Employee employee, string newPassword);
}