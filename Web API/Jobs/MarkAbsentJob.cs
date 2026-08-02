using Application.Interface;

namespace Web_API.Jobs;

public class MarkAbsentJob(IAttendanceRepository repository)
{
    private readonly IAttendanceRepository _repository = repository;

    public Task MarkAbsentJobAsync(DateOnly date, CancellationToken ct)
    {
        return _repository.MarkAbsentEmployeeAsync(date, ct);
    }
}
