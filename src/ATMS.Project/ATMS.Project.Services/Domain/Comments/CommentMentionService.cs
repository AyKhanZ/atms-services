using System.Text.RegularExpressions;
using ATMS.Project.Services.Domain.Comments.Interfaces;

namespace ATMS.Project.Services.Domain.Comments;

public sealed class CommentMentionService : ICommentMentionService
{
    private static readonly Regex MentionPattern = new(
        @"@\[user:([0-9a-fA-F-]{36})\]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    public Guid[] GetMentionedUserIds(string text)
    {
        return MentionPattern.Matches(text)
            .Select(match => Guid.TryParse(match.Groups[1].Value, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
    }
}
