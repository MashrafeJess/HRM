using Application.Interface;

namespace Web_API.Jobs;

public sealed class GeneratePayrollPerMonthBackgroundJob(IServiceScopeFactory scopeFactory, ILogger<GeneratePayrollPerMonthBackgroundJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Generate Payroll per month background job");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;

                if (ShouldRun(now))
                {
                    var year = now.Year;
                    var month = now.Month;
                    
                    using var scope = scopeFactory.CreateScope();
                    
                    var job = scope.ServiceProvider.GetRequiredService<GeneratePayrollJob>();
                    
                    await job.GeneratePayrolls(month, year, stoppingToken);
                    
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }else
                { 
                    await Task.Delay( TimeSpan.FromHours(24), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    private static bool ShouldRun(DateTime now)
    {
        return now is { Day: 1 };
    }
}