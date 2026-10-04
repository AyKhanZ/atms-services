using ATMS.Project.Services.Comments;

namespace Project.Services.Tests.Comments;

public class CommentMentionServiceTest
{
    private static readonly Guid First = Guid.Parse("8d4c1f3e-2b7a-4e21-9a55-0c6f1d2e3b4a");
    private static readonly Guid Second = Guid.Parse("1a2b3c4d-5e6f-4a1b-8c2d-3e4f5a6b7c8d");

    [Fact]
    public void GetMentionedUserIds_ReadsEveryMentionOnceInOrder()
    {
        var text = $"@[user:{First}] and @[user:{Second}], also @[user:{First}]";

        var ids = new CommentMentionService().GetMentionedUserIds(text);

        Assert.Equal([First, Second], ids);
    }

    [Theory]
    [InlineData("")]
    [InlineData("No mentions here")]
    [InlineData("@Aykhan Zeynalov typed by hand")]
    [InlineData("@[user:not-a-guid-not-a-guid-not-a-guid-xxxx]")]
    [InlineData("@[user:00000000-0000-0000-0000-000000000000]")]
    public void GetMentionedUserIds_WhenThereIsNoRealMention_ReturnsNothing(string text)
    {
        Assert.Empty(new CommentMentionService().GetMentionedUserIds(text));
    }
}
