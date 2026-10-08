using ATMS.Caching.Constants;

namespace ATMS.Caching.Tests;

public class CacheKeysTests
{
    [Theory]
    [InlineData("en")]
    [InlineData("ru")]
    [InlineData("az")]
    public void ProjectEntityDetailKeys_IncludeRequestedLanguage(string language)
    {
        var id = Guid.NewGuid();

        Assert.EndsWith($":{language}", CacheKeys.Project.ProjectById(id, language));
        Assert.EndsWith($":{language}", CacheKeys.Project.TicketById(id, language));
        Assert.EndsWith($":{language}", CacheKeys.Project.TaskById(id, language));
    }
}
