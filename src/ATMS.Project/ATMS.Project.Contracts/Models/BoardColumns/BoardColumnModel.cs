using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.BoardColumns;

public sealed class BoardColumnModel : DictionaryModel
{
    // with the status when the status has several columns: "In Progress · Done"; Name alone is for the board header
    public string Label { get; set; }

    public int Category { get; set; }

    public int Order { get; set; }
}
