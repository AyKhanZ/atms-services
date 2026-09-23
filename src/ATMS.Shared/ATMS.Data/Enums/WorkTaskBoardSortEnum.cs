namespace ATMS.Data.Enums;

/// <summary>What the Tasks page orders by; which way round comes from the request's sort direction.</summary>
public enum WorkTaskBoardSortEnum
{
    Rank = 1,

    DoneAt = 2,

    Deadline = 3,

    Priority = 4
}
