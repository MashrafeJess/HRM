namespace Domain.Models;

public sealed partial class Employee
{
    public Role? Role { get; set; }
    public Department? Department { get; set; }
}