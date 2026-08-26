using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Users;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Users;

[Access(PermissionEnum.ProjectEdit)]
public class GetProjectTeamMembersRequest : IRequest<UserModel[]>;
