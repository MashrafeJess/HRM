using Application.DTOs;
using Domain.Models;

namespace Application.Interface;

public interface ICompanyRepository
{
    Task<CompanyDto> EditCompany(Company company, CancellationToken ct);
    Task<Company?> GetCompanyById(long? companyId, CancellationToken ct);
    Task<List<CompanyDto>> GetCompanies(string viewOrder, int pageNumber, int pageSize, CancellationToken ct);
}