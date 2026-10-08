using ATMS.Project.Data.Criteria.WorkProjectParticipants;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.WorkProjectParticipants;

public class WorkTaskBoardAssigneesFilterTest
{
    private static readonly Guid First = Guid.NewGuid();
    private static readonly Guid Second = Guid.NewGuid();

    private static readonly WorkProjectParticipant[] Participants =
    [
        new() { UserId = Guid.NewGuid(), WorkProjectId = First },
        new() { UserId = Guid.NewGuid(), WorkProjectId = Second }
    ];

    [Fact]
    public void Apply_WithoutProjects_KeepsEveryone()
    {
        var result = new WorkTaskBoardAssigneesFilter().Apply(Participants.AsQueryable()).ToArray();

        Assert.Equal(2, result.Length);
    }

    [Fact]
    public void Apply_WithProjects_KeepsTheirParticipantsOnly()
    {
        var result = new WorkTaskBoardAssigneesFilter { ProjectIds = [Second] }
            .Apply(Participants.AsQueryable())
            .Select(participant => participant.WorkProjectId)
            .ToArray();

        Assert.Equal([Second], result);
    }
}
