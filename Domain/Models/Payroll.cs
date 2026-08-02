namespace Domain.Models;

public partial class Payroll
{
    public long PayrollId { get; set; }

    public long CompanyId { get; set; }

    public long EmployeeId { get; set; }

    public string Month { get; set; } = null!;

    public long Year { get; set; }

    public decimal BasicSalary { get; set; }

    public decimal? AbsentDeduction { get; set; }

    public decimal? LateDeduction { get; set; }

    public decimal? NetSalary { get; set; }

    public DateTime? GeneratedAt { get; set; }
}
