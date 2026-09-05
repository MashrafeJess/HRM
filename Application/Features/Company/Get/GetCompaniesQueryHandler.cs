using Application.Common;
using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Company.Get;

public class GetCompaniesQueryHandler(ICompanyRepository repository) : IRequestHandler<GetCompaniesQuery, PagedResult<CompanyDto>>
{
    private readonly ICompanyRepository _repository = repository;
    public async Task<PagedResult<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PaginationDefaults.Normalize(request.PageNumber, request.PageSize);
        return await _repository.GetCompanies(request.ViewOrder, pageNumber, pageSize, cancellationToken);
    }
}
