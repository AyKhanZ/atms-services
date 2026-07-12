using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Criteria.Users;
using ATMS.Admin.Data.Entities;
using AutoMapper;

namespace ATMS.Admin.Service.Mappers;

public class CommandToEntityProfile : Profile
{
    public CommandToEntityProfile()
    {
        CreateMap<RegisterCommand, User>();
        
        CreateMap<GetUsersRequest, UserFilter>();
    }
}
