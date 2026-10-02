using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.Comments;

public sealed class CommentModel
{
    public Guid Id { get; set; }

    public string Text { get; set; }

    public DateTime CreatedAt { get; set; }

    public PersonModel CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public PersonModel[] Mentions { get; set; } = [];

    public CommentReferenceModel[] References { get; set; } = [];
}
