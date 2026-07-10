using ATMS.Admin.Contracts.Models.UserProgresses;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.UserProgresses;

public class GetUserProgressRequest : IRequest<UserProgressModel>;