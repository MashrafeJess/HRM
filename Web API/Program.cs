using Application;
using Infrastructure;
using Microsoft.OpenApi;
using Web_API.Jobs;
using Web_API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your access token (without the \"Bearer \" prefix)."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
    });
});

//Add Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddScoped<MarkAbsentJob>();
builder.Services.AddScoped<GeneratePayrollJob>();
builder.Services.AddHostedService<MarkAbsentBackgroundService>();
builder.Services.AddHostedService<GeneratePayrollPerMonthBackgroundJob>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Must be registered before Swagger/Auth/MVC so it sits between them and the
// framework's automatic Development-mode exception page: ExceptionMiddleware was
// defined but never wired in, so every NotFoundException/BadRequestException/
// ForbiddenException thrown by handlers was falling through as a raw 500 with a
// full stack trace instead of the intended clean 404/400/403 JSON response.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
