using ATMS.Project.Data.Criteria.WorkProjectParticipants;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.WorkProjectParticipants;

public class ParticipantsOfMyProjectsCriteriaTest
{
    [Fact]
    public void Apply_KeepsOnlyParticipantsOfProjectsTheUserIsIn()
    {
        var me = Guid.NewGuid();
        var colleague = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var mine = Project(me, colleague);
        var theirs = Project(stranger);
        var participants = mine.WorkProjectParticipants.Concat(theirs.WorkProjectParticipants).AsQueryable();

        var result = new ParticipantsOfMyProjectsCriteria(me)
            .Apply(participants)
            .Select(participant => participant.UserId)
            .ToArray();

        Assert.Equal([me, colleague], result);
    }

    private static WorkProject Project(params Guid[] userIds)
    {
        var project = new WorkProject { Id = Guid.NewGuid() };
        project.WorkProjectParticipants = userIds
            .Select(userId => new WorkProjectParticipant
            {
                UserId = userId,
                WorkProjectId = project.Id,
                WorkProject = project
            })
            .ToList();
        return project;
    }
}
