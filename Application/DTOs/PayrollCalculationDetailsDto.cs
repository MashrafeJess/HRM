namespace Application.DTOs;

public class PayrollCalculationDetailsDto
{
    public decimal Salary { get; set; }
    public long LateAbsentCount { get; set; }
    public decimal AbsentPenalty {get; set;}
    public decimal LatePenalty { get; set; }
}