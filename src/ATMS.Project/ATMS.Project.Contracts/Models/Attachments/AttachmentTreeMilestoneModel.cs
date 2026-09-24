namespace ATMS.Project.Contracts.Models.Attachments;

public class AttachmentTreeMilestoneModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int FileCount { get; set; }
    public IReadOnlyCollection<AttachmentTreeTicketModel> Tickets { get; set; } = [];
}
