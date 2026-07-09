using Application.DTOs;
using MediatR;

namespace Application.Features.Company.GetById;

public record GetCompanyByIdQuery(long CompanyId) : IRequest<CompanyDto>;