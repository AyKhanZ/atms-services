using ATMS.Project.Contracts.Models.Organization;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Data.Entities;
using AutoMapper;

namespace ATMS.Project.Services.Mappers;

public class EntityToModelProfile : Profile
{
    public EntityToModelProfile()
    {
        CreateMap<Organization, OrganizationModel>();
        
        CreateMap<Organization, OrganizationItemModel>();
        
        
        CreateMap<User, UserModel>();
    }
}
