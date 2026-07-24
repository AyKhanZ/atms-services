using ATMS.Admin.Service.Infrastructure.Delivery;

namespace Admin.Services.Tests.Infrastructure.Delivery;

public class DeliveryRetryScheduleTest
{
    [Fact]
    public void MaxAttemptCount_ReturnsTen()
    {
        var schedule = new DeliveryRetrySchedule();

        Assert.Equal(10, schedule.MaxAttemptCount);
    }

    [Theory]
    [InlineData(1, 30)]
    [InlineData(2, 120)]
    [InlineData(3, 300)]
    [InlineData(9, 28800)]
    public void GetNextAttemptAt_ReturnsConfiguredDelay(int attemptCount, int expectedSeconds)
    {
        var schedule = new DeliveryRetrySchedule();
        var before = DateTime.UtcNow.AddSeconds(expectedSeconds);

        var result = schedule.GetNextAttemptAt(attemptCount);

        var after = DateTime.UtcNow.AddSeconds(expectedSeconds);
        Assert.InRange(result, before, after);
    }
}
