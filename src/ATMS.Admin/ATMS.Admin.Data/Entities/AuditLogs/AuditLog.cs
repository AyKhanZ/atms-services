namespace ATMS.Admin.Data.Entities.AuditLogs;

public class AuditLog : BaseDocument
{
    public UserBase CreatedBy { get; set; }
    public DateTime EventDate { get; set; }
    public string IPAddress { get; set; }
}