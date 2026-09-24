using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.Attachments;

public class AttachmentModel
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public AttachmentAuthorModel CreatedBy { get; set; }
    public DictionaryModel<Guid> WorkTask { get; set; }
    public DictionaryModel<Guid>? ParentWorkTask { get; set; }
}
