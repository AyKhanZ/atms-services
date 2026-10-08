using ATMS.Application.Models;
using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.Attachments;

public sealed class AttachmentModel
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public PersonModel CreatedBy { get; set; }
    public DictionaryModel<Guid> WorkTask { get; set; }
    public DictionaryModel<Guid>? ParentWorkTask { get; set; }
}
