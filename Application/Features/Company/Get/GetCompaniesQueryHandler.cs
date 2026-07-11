using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Company.Get;

public class GetCompaniesQueryHandler(ICompanyRepository repository) : IRequestHandler<GetCompaniesQuery,List<CompanyDto>>
{
    private readonly ICompanyRepository _repository = repository;
    public async Task<List<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _repository.GetCompanies(request.ViewOrder,request.PageNumber, request.PageSize,
            cancellationToken);
        return companies;
    }
}