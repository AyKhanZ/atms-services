namespace ATMS.Project.Services.Domain.Board.Interfaces;

public interface IWorkTaskBoardPositionService
{
    int MaxLength { get; }

    // null on either side = the end of the column
    string Between(string? above, string? below);
}
