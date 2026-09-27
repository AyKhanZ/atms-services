using ATMS.Project.Contracts.Models.History;

namespace ATMS.Project.Contracts.Models.Comments;

public sealed class CommentModel
{
    public Guid Id { get; set; }

    public string Text { get; set; }

    public DateTime CreatedAt { get; set; }

    public HistoryPersonModel CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public HistoryPersonModel[] Mentions { get; set; } = [];

    public CommentReferenceModel[] References { get; set; } = [];
}
