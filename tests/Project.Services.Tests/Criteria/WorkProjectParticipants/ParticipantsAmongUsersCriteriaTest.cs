using ATMS.Project.Data.Criteria.WorkProjectParticipants;
using ATMS.Project.Data.Entities;

namespace Project.Services.Tests.Criteria.WorkProjectParticipants;

public class ParticipantsAmongUsersCriteriaTest
{
    [Fact]
    public void Apply_KeepsOnlyParticipantsWhoseUserIsInTheGivenUsers()
    {
        var employee = new User { Id = Guid.NewGuid() };
        var client = new User { Id = Guid.NewGuid() };
        var participants = new[]
        {
            new WorkProjectParticipant { UserId = employee.Id },
            new WorkProjectParticipant { UserId = client.Id }
        }.AsQueryable();

        var result = new ParticipantsAmongUsersCriteria(new[] { employee }.AsQueryable())
            .Apply(participants)
            .Select(participant => participant.UserId)
            .ToArray();

        Assert.Equal([employee.Id], result);
    }
}
