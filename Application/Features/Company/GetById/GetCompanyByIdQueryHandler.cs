using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Company.GetById;

public class GetCompanyByIdQueryHandler(ICompanyRepository repository) : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository = repository;
    
    public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetCompanyById(request.CompanyId, cancellationToken);
        if (company == null)
        {
            throw new InvalidDataException("Company not found");    
        }

        return new CompanyDto
        {
            CompanyId = company.CompanyId,
            CompanyAddress =  company.CompanyAddress,
            CompanyName = company.CompanyName,
            IsActive = company.IsActive,
            LogoUrl =  company.LogoUrl,
            SubscriptionPlan =  company.SubscriptionPlan,
            CompanyEmail =  company.CompanyEmail,
            CompanyPhone =  company.CompanyPhone,
            CreatedAt = company.CreatedAt,
            UpdatedAt = company.UpdatedAt,
        };
    }
}