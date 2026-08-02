using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Criteria.Users;
using AutoMapper;

namespace ATMS.Admin.Service.Mappers;

public class RequestToFilterProfile : Profile
{
    public RequestToFilterProfile()
    {
        CreateMap<GetUsersRequest, UserFilter>();
    }
}
