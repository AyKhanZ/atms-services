namespace ATMS.Project.Contracts.Models.Attachments;

public class AttachmentTreeModel
{
    public int FileCount { get; set; }
    public IReadOnlyCollection<AttachmentTreeGroupModel> Groups { get; set; } = [];
}
