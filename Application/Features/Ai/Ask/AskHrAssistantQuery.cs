using Application.DTOs;
using MediatR;

namespace Application.Features.Ai.Ask;

public record AskHrAssistantQuery(string Question, long CompanyId) : IRequest<AiAnswerDto>;
