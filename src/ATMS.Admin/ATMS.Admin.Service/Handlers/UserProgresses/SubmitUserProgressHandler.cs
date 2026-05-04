using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Application.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.UserProgresses;

public class SubmitUserProgressHandler(
    ICurrentUser currentUser) : IRequestHandler<SubmitUserProgressCommand>
{
    public Task Handle(SubmitUserProgressCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}