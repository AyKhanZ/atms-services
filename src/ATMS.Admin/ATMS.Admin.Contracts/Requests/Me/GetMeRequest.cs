using ATMS.Admin.Contracts.Models.Me;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Me;

public class GetMeRequest : IRequest<MeModel>;
