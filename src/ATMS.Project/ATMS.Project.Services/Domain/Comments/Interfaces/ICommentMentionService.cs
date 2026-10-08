namespace ATMS.Project.Services.Domain.Comments.Interfaces;

public interface ICommentMentionService
{
    Guid[] GetMentionedUserIds(string text);
}
