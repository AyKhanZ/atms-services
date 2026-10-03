namespace ATMS.Project.Services.Comments.Interfaces;

public interface ICommentMentionService
{
    Guid[] GetMentionedUserIds(string text);
}
