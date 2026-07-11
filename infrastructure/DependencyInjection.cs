using Application.Interface;
using Domain;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // ✅ DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config
                .GetConnectionString("DefaultConnection")));
        
        // ✅ Contexts
        services.AddScoped<IAppDbContext, AppDbContext>();
        // ✅ Repositories
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();

        // ✅ Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();

        return services;
    }
}