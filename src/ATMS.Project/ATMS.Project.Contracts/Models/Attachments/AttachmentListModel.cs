namespace ATMS.Project.Contracts.Models.Attachments;

public sealed class AttachmentListModel
{
    public IReadOnlyCollection<AttachmentModel> Items { get; set; } = [];
    public bool HasMore { get; set; }
}
