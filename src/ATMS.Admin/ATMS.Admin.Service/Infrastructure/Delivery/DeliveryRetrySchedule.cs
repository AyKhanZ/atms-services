namespace ATMS.Admin.Service.Infrastructure.Delivery;

public class DeliveryRetrySchedule
{
    private readonly TimeSpan[] _delays =
    [
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(2),
        TimeSpan.FromHours(4),
        TimeSpan.FromHours(8)
    ];

    public int MaxAttemptCount => _delays.Length + 1;

    public DateTime GetNextAttemptAt(int attemptCount)
    {
        return DateTime.UtcNow.Add(_delays[attemptCount - 1]);
    }
}
