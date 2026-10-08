using ATMS.Admin.Contracts.Models.Me;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Me;

public sealed class GetMeRequest : IRequest<MeModel>;
