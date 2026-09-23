namespace ATMS.Project.Services.Board.Interfaces;

public interface IWorkTaskBoardPositionService
{
    /// <summary>The longest key a position may take.</summary>
    int MaxLength { get; }

    /// <summary>
    /// The key of a card dropped between two neighbours. Null on either side means the end of the
    /// column: no card above, or no card below.
    /// </summary>
    string Between(string? above, string? below);
}
