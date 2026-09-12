using Application.Interface;

namespace Web_API.Jobs;

public class GeneratePayrollJob(IPayrollRepository repository)
{
    private readonly IPayrollRepository _repository = repository;

    public Task GeneratePayrolls(long monthId, long yearId, CancellationToken ct)
    {
        return _repository.GeneratePayrollsAsync(monthId, yearId, ct);
    }

    public async Task EnsurePayrollGeneratedAsync(long monthId, long yearId, CancellationToken ct)
    {
        if (await _repository.HasPayrollBeenGeneratedAsync(monthId, yearId, ct))
        {
            return;
        }

        await _repository.GeneratePayrollsAsync(monthId, yearId, ct);
    }
}
