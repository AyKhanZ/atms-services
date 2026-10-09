using ATMS.Admin.Contracts.Models.Localization;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Localization;

public sealed class GetDefaultLanguageRequest : IRequest<DefaultLanguageModel>;
