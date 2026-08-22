using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

public class CreateWorkProjectCommand : WorkProjectCommand, IRequest<Guid>;
