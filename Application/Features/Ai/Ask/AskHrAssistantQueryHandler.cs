using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Ai.Ask;

public class AskHrAssistantQueryHandler(IHrAssistantService assistant)
    : IRequestHandler<AskHrAssistantQuery, AiAnswerDto>
{
    private readonly IHrAssistantService _assistant = assistant;

    public Task<AiAnswerDto> Handle(AskHrAssistantQuery request, CancellationToken cancellationToken)
        => _assistant.AskAsync(request.Question, request.CompanyId, cancellationToken);
}
