using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Tokens;

namespace Admin.Services.Tests.Data;

public class UserSessionConfigurationTest
{
    [Fact]
    public void Model_ConfiguresRequiredIndexesAndConcurrencyToken()
    {
        using var context = new AdminDbContext();
        var entity = context.Model.FindEntityType(typeof(UserSession));

        Assert.NotNull(entity);
        Assert.True(entity.FindProperty(nameof(UserSession.RevokedAt))?.IsConcurrencyToken);

        var indexes = entity.GetIndexes().ToArray();
        Assert.Contains(indexes, index =>
            index.IsUnique
            && index.Properties.Single().Name == nameof(UserSession.TokenHash));
        Assert.Contains(indexes, index =>
            index.Properties.Single().Name == nameof(UserSession.UserId));
        Assert.Contains(indexes, index =>
            index.Properties.Single().Name == nameof(UserSession.FamilyId));
        Assert.Contains(indexes, index =>
            index.Properties.Single().Name == nameof(UserSession.FamilyExpiresAt));
    }
}
