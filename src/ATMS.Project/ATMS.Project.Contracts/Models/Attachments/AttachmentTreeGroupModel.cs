namespace ATMS.Project.Contracts.Models.Attachments;

public class AttachmentTreeGroupModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int FileCount { get; set; }
    public IReadOnlyCollection<AttachmentTreeMilestoneModel> Milestones { get; set; } = [];
}
