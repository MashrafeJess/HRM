using Application.DTOs;
using Application.Interface;
using MediatR;

namespace Application.Features.Company.CreateOrUpdate;

public class CompanyUpSertCommandHandler(ICompanyRepository repository)
    : IRequestHandler<CreateOrUpdateCompanyUpSertCommand, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository = repository;

    public async Task<CompanyDto> Handle(CreateOrUpdateCompanyUpSertCommand request,
        CancellationToken cancellationToken)
    {
        Domain.Models.Company company;

        if (request.Dto.CompanyId is null or 0)
        {
            company = new Domain.Models.Company
            {
                CompanyAddress = request.Dto.CompanyAddress,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                LogoUrl = request.Dto.LogoUrl,
                SubscriptionPlan = request.Dto.SubscriptionPlan,
                CompanyEmail = request.Dto.CompanyEmail,
                CompanyName = request.Dto.CompanyName,
                CompanyPhone = request.Dto.CompanyPhone,
            };
        }
        else
        {
            company = await _companyRepository.GetCompanyById(request.Dto.CompanyId, cancellationToken)
                       ?? throw new InvalidOperationException($"Company with id {request.Dto.CompanyId} does not exist.");

            company.CompanyAddress = request.Dto.CompanyAddress;
            company.IsActive = request.Dto.IsActive ?? company.IsActive;
            company.LogoUrl = request.Dto.LogoUrl;
            company.SubscriptionPlan = request.Dto.SubscriptionPlan;
            company.CompanyEmail = request.Dto.CompanyEmail;
            company.CompanyName = request.Dto.CompanyName;
            company.CompanyPhone = request.Dto.CompanyPhone;
            company.UpdatedAt = DateTime.UtcNow;
        }

        await _companyRepository.EditCompany(company, cancellationToken);

        return MapToDto(company);
    }

    private static CompanyDto MapToDto(Domain.Models.Company company) => new()
    {
        CompanyAddress = company.CompanyAddress,
        CompanyEmail = company.CompanyEmail,
        CompanyName = company.CompanyName,
        CompanyPhone = company.CompanyPhone,
        IsActive = company.IsActive,
        LogoUrl = company.LogoUrl,
        SubscriptionPlan = company.SubscriptionPlan,
    };
}