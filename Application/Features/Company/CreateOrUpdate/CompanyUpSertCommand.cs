using Application.DTOs;
using MediatR;

namespace Application.Features.Company.CreateOrUpdate;

public record CreateOrUpdateCompanyUpSertCommand(CompanyDto Dto) : IRequest<CompanyDto>;