using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Criteria.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using ATMS.Data.Criteria;
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
        var criteria = filter.And(new NotAdminCriteria());
        
        var pagination = new PaginationCriteria<User>(request.Page, request.PageSize);
        
        var users = await userRepository.GetAsync(criteria, pagination, cancellationToken);

        return users.Map(user =>
        {
            var model = mapper.Map<UserListItemModel>(user);
            model.UserStatus = user.UserStatus
                .ToDictionaryModel(user.UserStatus.Translations, CultureHelper.CurrentLanguage);
            return model;
        });
    }
}
