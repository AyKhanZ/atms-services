using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.Attachments;

public sealed class AttachmentTreeTicketModel
{
    public DictionaryModel<Guid> WorkTicket { get; set; }
    public int FileCount { get; set; }
}
