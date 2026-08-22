using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Data.Services.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkProjects;

public class CreateWorkProjectHandler(
    ICurrentUser currentUser,
    IMapper mapper,
    IWorkProjectRepository workProjectRepository,
    IEntityCodeGenerator codeGenerator)
    : IRequestHandler<CreateWorkProjectCommand, Guid>
{
    public async Task<Guid> Handle(CreateWorkProjectCommand command, CancellationToken cancellationToken)
    {
        var project = mapper.Map<WorkProject>(command);
        project.Id = Guid.NewGuid();
        project.Code = await codeGenerator.GetNextAsync(cancellationToken);
        project.Title = command.Title.Trim();
        project.CreatedById = currentUser.Id;
        project.WorkProjectParticipants = command.Participants.Select(CreateParticipant).ToArray();

        await workProjectRepository.CreateAsync(project, cancellationToken);

        return project.Id;
    }

    private WorkProjectParticipant CreateParticipant(WorkProjectParticipantCommand command)
    {
        var participant = new WorkProjectParticipant
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId
        };

        participant.WorkProjectParticipantRoles.Add(new WorkProjectParticipantRole
        {
            Id = Guid.NewGuid(),
            RoleId = command.RoleId
        });

        return participant;
    }
}
