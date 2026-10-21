using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities;

public class BoardColumnTranslation : BaseEntity<int>, ITranslation
{
    public int BoardColumnId { get; set; }

    public string Language { get; set; }

    public string Name { get; set; }

    public BoardColumn BoardColumn { get; set; }
}
