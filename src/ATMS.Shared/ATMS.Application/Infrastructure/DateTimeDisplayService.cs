using ATMS.Application.Interfaces;

namespace ATMS.Application.Infrastructure;

public class DateTimeDisplayService : IDateTimeDisplayService
{
    private static readonly TimeZoneInfo BakuTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku");

    public DateTime ToBakuDateTime(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
            BakuTimeZone);
    }
}
