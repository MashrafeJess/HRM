using Application.DTOs;
using Application.Interface;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CompanyRepository(IAppDbContext dbContext)
{
    private readonly IAppDbContext _appDbContext = dbContext;

    public async Task<CompanyDto> EditCompany(Company company, CancellationToken ct)
    {
        if (company.CompanyId is 0)
        {
            await _appDbContext.Companies.AddAsync(company, ct);
        }
        else
        {
            _appDbContext.Companies.Update(company);
        }

        await _appDbContext.SaveChangesAsync(ct);
        return new CompanyDto()
        {
            CompanyAddress = company.CompanyAddress,
            CompanyName = company.CompanyName,
            CompanyPhone = company.CompanyPhone,
            CompanyEmail = company.CompanyEmail,
            IsActive = company.IsActive,
            LogoUrl = company.LogoUrl,
            SubscriptionPlan = company.SubscriptionPlan,
        };
    }

    public async Task<Company?> GetCompany(long companyId, CancellationToken ct)
    {
        var company = await _appDbContext.Companies.FindAsync([companyId, ct], cancellationToken: ct);
        return company;
    }

    public async Task<List<CompanyDto>> GetCompanies(string? viewOrder, int pageNumber, int pageSize, CancellationToken ct)
    {
        IQueryable<Company> query = _appDbContext.Companies;
        query = viewOrder?.ToLower()switch
        {
            "desc" => query.OrderByDescending(c => c.CreatedAt),
            _ => query.OrderBy(c => c.CreatedAt)
        };

        var companies = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyDto
            {
                CompanyAddress =  c.CompanyAddress,
                CompanyEmail =  c.CompanyEmail,
                IsActive = c.IsActive,
                LogoUrl = c.LogoUrl,
                SubscriptionPlan =  c.SubscriptionPlan,
                CompanyName =  c.CompanyName,
                CompanyPhone =  c.CompanyPhone,
            })
            .ToListAsync(ct);
        return companies;
    }

    public async Task<Company?> GetCompanyById(long companyId, CancellationToken ct)
    {
        var company = await _appDbContext.Companies.FindAsync([companyId, ct], ct);
        return company;
    }
}