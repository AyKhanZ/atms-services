namespace ATMS.Application.Interfaces;

public interface ICurrentUser
{
    Guid Id { get; }
    Guid RoleId { get; }
    string UserType { get; }
    bool HasCompletedSurvey { get; }
    bool EmailConfirmed { get; }
}
