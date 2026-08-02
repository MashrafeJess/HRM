using Application.Common.Constants;
using Microsoft.Extensions.Hosting;

namespace Web_API.Jobs;

public sealed class MarkAbsentBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<MarkAbsentBackgroundService> logger) : BackgroundService
{
    private static readonly TimeZoneInfo BangladeshTimeZone = GetBangladeshTimeZone();

    private static readonly TimeSpan RunTime = new(9, 30, 0);

    private static TimeZoneInfo GetBangladeshTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Bangladesh Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Mark-absent background service started. It runs daily at 9:30 AM Bangladesh time.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowUtc = DateTime.UtcNow;
            var nowBangladesh = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, BangladeshTimeZone);
            var nextRunDate = nowBangladesh.TimeOfDay < RunTime
                ? nowBangladesh.Date
                : nowBangladesh.Date.AddDays(1);

            var nextRunLocal = nextRunDate.Add(RunTime);
            var nextRunUtc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(nextRunLocal, DateTimeKind.Unspecified),
                BangladeshTimeZone);
            var delay = nextRunUtc - nowUtc;

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            var attendanceDate = DateOnly.FromDateTime(nextRunLocal);
            
            if (WeeklyHolidayCalendar.IsHoliday(attendanceDate))
            {
                logger.LogInformation(
                    "Skipping {Date} because it is a weekly holiday.",
                    attendanceDate);

                continue;
            }

            try
            {
                using var scope = scopeFactory.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<MarkAbsentJob>();

                await job.MarkAbsentJobAsync(attendanceDate, stoppingToken);

                logger.LogInformation(
                    "Mark-absent job completed for {AttendanceDate}.", attendanceDate);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Mark-absent job failed for {AttendanceDate}.", attendanceDate);
            }
        }
    }
}
