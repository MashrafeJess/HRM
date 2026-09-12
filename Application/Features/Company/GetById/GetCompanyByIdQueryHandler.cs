using Application.Common.Exceptions;
using Application.DTOs;
using Application.Interface;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Application.Features.Company.GetById;

public class GetCompanyByIdQueryHandler(ICompanyRepository repository, IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository = repository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        // "Company Admin" may only view their own company; [Authorize(Roles = "Super
        // Admin, Company Admin")] on the controller can't express that restriction,
        // so it's enforced here using the CompanyId/Role claims from the token.
        var user = _httpContextAccessor.HttpContext?.User;
        var role = user?.FindFirstValue("Role") ?? "";
        if (role != "Super Admin")
        {
            var callerCompanyIdRaw = user?.FindFirstValue("CompanyId");
            if (!long.TryParse(callerCompanyIdRaw, out var callerCompanyId) || callerCompanyId != request.CompanyId)
            {
                throw new ForbiddenException("A Company Admin can only view their own company.");
            }
        }

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