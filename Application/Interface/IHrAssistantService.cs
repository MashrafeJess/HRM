using Application.DTOs;

namespace Application.Interface;

public interface IHrAssistantService
{
    Task<AiAnswerDto> AskAsync(string question, long companyId, CancellationToken ct);
}
