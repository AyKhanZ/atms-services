using ATMS.Data.Constants;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Contracts.Models.WorkTaskBoard;
using ATMS.Project.Contracts.Requests.WorkTaskBoard;
using ATMS.Project.Data.Criteria.WorkProjectParticipants;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Handlers.WorkTaskBoard;
using Moq;

namespace Project.Services.Tests.Handlers.WorkTaskBoard;

public class GetWorkTaskBoardAssigneesHandlerTest : BaseHandlerTest
{
    private readonly Guid _me = Guid.NewGuid();
    private readonly Guid _colleague = Guid.NewGuid();
    private readonly Guid _stranger = Guid.NewGuid();

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 3)]
    public async Task Handle_OffersPeopleOfMyProjects_OrEveryoneToASuperAdmin(bool superAdmin, int expected)
    {
        CurrentUserMock.SetupGet(user => user.Id).Returns(_me);
        CurrentUserMock.SetupGet(user => user.RoleId).Returns(superAdmin ? RoleIds.SuperAdmin : RoleIds.Employee);
        MapperMock.Setup(mapper => mapper.Map<WorkTaskBoardAssigneesFilter>(It.IsAny<GetWorkTaskBoardAssigneesRequest>()))
            .Returns(new WorkTaskBoardAssigneesFilter());
        MapperMock.Setup(mapper => mapper.Map<WorkTaskBoardAssigneeModel[]>(It.IsAny<WorkTaskBoardAssignee[]>()))
            .Returns<WorkTaskBoardAssignee[]>(people => people
                .Select(person => new WorkTaskBoardAssigneeModel { Id = person.UserId })
                .ToArray());
        var participants = Participants();
        var repository = new Mock<IWorkTaskBoardRepository>();
        repository.Setup(repo => repo.GetAssigneesAsync(It.IsAny<ICriteria<WorkProjectParticipant>>(), It.IsAny<CancellationToken>()))
            .Returns<ICriteria<WorkProjectParticipant>, CancellationToken>((criteria, _) => Task.FromResult(
                criteria.Apply(participants.AsQueryable())
                    .Select(participant => new WorkTaskBoardAssignee(participant.UserId, "Name", "Surname", null))
                    .ToArray()));
        var handler = new GetWorkTaskBoardAssigneesHandler(CurrentUserMock.Object, repository.Object, MapperMock.Object);

        var result = await handler.Handle(new GetWorkTaskBoardAssigneesRequest(), CancellationToken.None);

        Assert.Equal(expected, result.Length);
        Assert.Equal(superAdmin, result.Any(person => person.Id == _stranger));
    }

    private WorkProjectParticipant[] Participants()
    {
        var mine = new WorkProject { Id = Guid.NewGuid() };
        var theirs = new WorkProject { Id = Guid.NewGuid() };
        mine.WorkProjectParticipants =
        [
            new() { UserId = _me, WorkProject = mine, WorkProjectId = mine.Id },
            new() { UserId = _colleague, WorkProject = mine, WorkProjectId = mine.Id }
        ];
        theirs.WorkProjectParticipants =
        [
            new() { UserId = _stranger, WorkProject = theirs, WorkProjectId = theirs.Id }
        ];
        return [.. mine.WorkProjectParticipants, .. theirs.WorkProjectParticipants];
    }
}
