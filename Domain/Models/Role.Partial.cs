namespace Domain.Models;

public sealed partial class Role
{
    public ICollection<Employee> Employees { get; init; } = new List<Employee>();
}