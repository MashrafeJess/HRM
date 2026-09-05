using Application.Interface;
using Infrastructure.DbContexts;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<ILeaveRepository, LeaveRepository>();
        services.AddScoped<IPayrollRepository, PayrollRepository>();

        // ✅ Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IPayrollService, PayrollCalculateService>();

        // ✅ Authentication
        var jwt = config.GetSection("JwtSettings");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim types exactly as issued (e.g. "Name", "Role") instead of
                // letting the default inbound map rewrite short names back to
                // ClaimTypes.* URIs (http://schemas.xmlsoap.org/...).
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwt["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is missing."))),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    // Our tokens carry the role under the plain "Role" claim type
                    // (see TokenService.GenerateAccessToken), not ClaimTypes.Role,
                    // so [Authorize(Roles = "...")] needs to know to look there.
                    RoleClaimType = "Role",
                    NameClaimType = "Name"
                };
            });

        services.AddAuthorization();

        return services;
    }
}
