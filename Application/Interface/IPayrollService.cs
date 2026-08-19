using Application.DTOs;

namespace Application.Interface;

public interface IPayrollService
{
    public PayrollCalculationDetailsDto CalculatePayroll(decimal salary, long lates, long absents);
}