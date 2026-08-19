using Application.DTOs;
using Application.Interface;

namespace Infrastructure.Services;

public sealed class PayrollCalculateService : IPayrollService
{
    public PayrollCalculationDetailsDto CalculatePayroll(decimal salary, long lates, long absents)
    {
        const long penalty = 100;
        
        var lateAbsents = (lates < 3) ? 0 : (lates - (lates % 3)) / 3;

        var totalAbsents = (absents + lateAbsents) * penalty;

        return new PayrollCalculationDetailsDto
        {
            Salary = salary - totalAbsents,
            AbsentPenalty =  absents * penalty,
            LateAbsentCount = lateAbsents,
            LatePenalty = lateAbsents *  penalty,
        };
    }
}
