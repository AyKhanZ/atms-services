using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Users;

public class GetUsersHandler(
    IUserRepository userRepository,
    IMapper mapper
    ) : IRequestHandler<GetUsersRequest, UserListItemModel[]>
{
    public async Task<UserListItemModel[]> Handle(GetUsersRequest request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAsync(cancellationToken);
        
        var models = users.Select(user =>
        {
            var model = mapper.Map<UserListItemModel>(user);
            model.UserStatus = user.UserStatus.ToDictionaryModel(user.UserStatus.Translations, CultureHelper.CurrentLanguage);
            return model;
        }).ToArray();

        return models;
    }
}
