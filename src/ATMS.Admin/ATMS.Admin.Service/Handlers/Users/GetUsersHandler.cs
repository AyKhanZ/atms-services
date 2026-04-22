using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Criterias.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using ATMS.Data.Criterias;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Users;

public class GetUsersHandler(
    IUserRepository userRepository,
    IMapper mapper
) : IRequestHandler<GetUsersRequest, PagedResult<UserListItemModel>>
{
    public async Task<PagedResult<UserListItemModel>> Handle(GetUsersRequest request,
        CancellationToken cancellationToken)
    {
        var filter = mapper.Map<UserFilter>(request);
        
        var pagination = new PaginationCriteria<User>(request.Page, request.PageSize);
        
        var users = await userRepository.GetAsync(filter, pagination, cancellationToken);

        return users.Map(user =>
        {
            var model = mapper.Map<UserListItemModel>(user);
            model.UserStatus = user.UserStatus
                .ToDictionaryModel(user.UserStatus.Translations, CultureHelper.CurrentLanguage);
            return model;
        });
    }
}