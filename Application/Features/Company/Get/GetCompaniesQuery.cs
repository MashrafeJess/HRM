using Application.DTOs;
using MediatR;

namespace Application.Features.Company.Get;

public sealed record GetCompaniesQuery(string? ViewOrder, int? PageNumber, int? PageSize) : IRequest<PagedResult<CompanyDto>>;
