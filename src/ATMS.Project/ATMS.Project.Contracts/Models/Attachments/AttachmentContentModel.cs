namespace ATMS.Project.Contracts.Models.Attachments;

public class AttachmentContentModel
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public string PhysicalPath { get; set; }
    public bool CanPreview { get; set; }
}
