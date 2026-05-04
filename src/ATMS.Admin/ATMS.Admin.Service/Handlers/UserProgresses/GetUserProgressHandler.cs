using ATMS.Admin.Contracts.Models.UserProgresses;
using ATMS.Admin.Contracts.Requests.UserProgresses;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.UserProgresses;

public class GetUserProgressHandler(
    ICurrentUser currentUser,
    IUserProgressRepository userProgressRepository,
    IMapper mapper) : IRequestHandler<GetUserProgressRequest, UserProgressModel>
{
    public async Task<UserProgressModel> Handle(GetUserProgressRequest request, CancellationToken cancellationToken)
    {
        var progress = await userProgressRepository.GetAsync(currentUser.Id, cancellationToken);
            
        if (progress is null)
        {
            return new UserProgressModel { UserProgressType = currentUser.UserType };
        }

        return mapper.Map<UserProgressModel>(progress);
    }
}