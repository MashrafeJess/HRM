using Application.Common.Exceptions;
using Application.DTOs;
using Application.Interface;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Features.Company.CreateOrUpdate;

public class CompanyUpSertCommandHandler(ICompanyRepository repository, IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<CreateOrUpdateCompanyUpSertCommand, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository = repository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<CompanyDto> Handle(CreateOrUpdateCompanyUpSertCommand request,
        CancellationToken cancellationToken)
    {
        // Only "Super Admin" may create a new company or edit a company other than
        // their own. "Company Admin" may only edit the single company tied to their
        // account. [Authorize(Roles = "Super Admin, Company Admin")] on the controller
        // only checks role membership — it can't express "and only their own company" —
        // so that part is enforced here using the CompanyId/Role claims from the token.
        var user = _httpContextAccessor.HttpContext?.User;
        var role = user?.FindFirstValue("Role") ?? "";
        var isSuperAdmin = role == "Super Admin";

        if (!isSuperAdmin)
        {
            if (request.Dto.CompanyId is null or 0)
            {
                throw new ForbiddenException("Only a Super Admin can create a new company.");
            }

            var callerCompanyIdRaw = user?.FindFirstValue("CompanyId");
            if (!long.TryParse(callerCompanyIdRaw, out var callerCompanyId) || callerCompanyId != request.Dto.CompanyId)
            {
                throw new ForbiddenException("A Company Admin can only edit their own company.");
            }
        }

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