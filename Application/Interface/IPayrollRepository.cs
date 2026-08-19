using Domain.Models;

namespace Application.Interface;

public interface  IPayrollRepository
{
    Task<List<Payroll>> GeneratePayrollsAsync(long monthId, long yearId, CancellationToken cancellationToken);

    Task<Payroll> GetPayrollForEmployee(long employeeId, long yearId, long monthId,
        CancellationToken cancellationToken);

    Task<List<Payroll>> GetPayrollsForCompany(long companyId, long monthId, long yearId,
        CancellationToken cancellationToken);
}