using Application.Interface;

namespace Web_API.Jobs;

public sealed class GeneratePayrollPerMonthBackgroundJob(IServiceScopeFactory scopeFactory, ILogger<GeneratePayrollPerMonthBackgroundJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Generate Payroll per month background job started.");

        await EnsurePreviousMonthPayrollAsync(stoppingToken, "Startup catch-up");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (ShouldRun(DateTime.Now))
                {
                    await EnsurePreviousMonthPayrollAsync(stoppingToken, "Scheduled run");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Generate-payroll job failed.");
            }
        }
    }

    private async Task EnsurePreviousMonthPayrollAsync(CancellationToken stoppingToken, string trigger)
    {
        var previousMonth = DateTime.Now.AddMonths(-1);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<GeneratePayrollJob>();

            await job.EnsurePayrollGeneratedAsync(previousMonth.Month, previousMonth.Year, stoppingToken);

            logger.LogInformation(
                "{Trigger}: payroll ensured for {Month}/{Year}.", trigger, previousMonth.Month, previousMonth.Year);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, "{Trigger}: payroll generation failed for {Month}/{Year}.", trigger, previousMonth.Month, previousMonth.Year);
        }
    }

    private static bool ShouldRun(DateTime now) => now.Day == 1;
}
