namespace ATMS.Project.Contracts.Requests.Security;

public interface IProjectParticipantScopedRequest : IProjectScopedRequest
{
    Guid ParticipantId { get; }
}
