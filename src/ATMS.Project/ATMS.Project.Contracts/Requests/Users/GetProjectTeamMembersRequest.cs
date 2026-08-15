using ATMS.Project.Contracts.Models.Users;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Users;

public class GetProjectTeamMembersRequest : IRequest<UserModel[]>;
