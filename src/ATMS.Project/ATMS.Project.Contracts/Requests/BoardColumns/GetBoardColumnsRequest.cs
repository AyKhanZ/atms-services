using ATMS.Project.Contracts.Models.BoardColumns;
using MediatR;

namespace ATMS.Project.Contracts.Requests.BoardColumns;

public sealed class GetBoardColumnsRequest : IRequest<BoardColumnModel[]>;
