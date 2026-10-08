namespace ATMS.Project.Contracts.Models.Attachments;

public sealed class AttachmentTreeModel
{
    public int FileCount { get; set; }
    public IReadOnlyCollection<AttachmentTreeGroupModel> Groups { get; set; } = [];
}
