using ATMS.Data;

namespace ATMS.Project.Data.Entities;

// fixed set seeded by migration; a column is a part of one status, the task's status is the column's category.
// New, In Progress · Doing and Done carry the ids of their statuses: that is where a task lands when its
// status changes outside the board
public class BoardColumn : TranslatableDictionaryEntity
{
    public int Category { get; set; }

    // place on the board across all statuses
    public int Order { get; set; }

    public ICollection<BoardColumnTranslation> Translations { get; set; } = [];
}
