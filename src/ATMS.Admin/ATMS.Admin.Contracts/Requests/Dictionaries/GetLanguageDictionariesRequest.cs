using ATMS.Admin.Contracts.Models.Dictionaries;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Dictionaries;

public class GetLanguageDictionariesRequest : IRequest<LanguageModel[]>;
