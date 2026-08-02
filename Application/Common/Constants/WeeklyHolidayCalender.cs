namespace Application.Common.Constants;

public static class WeeklyHolidayCalendar
{
    public static readonly IReadOnlySet<DayOfWeek> Days =
        new HashSet<DayOfWeek>
        {
            DayOfWeek.Friday,
            DayOfWeek.Saturday
        };

    public static bool IsHoliday(DateOnly date)
    {
        return Days.Contains(date.DayOfWeek);
    }
}
