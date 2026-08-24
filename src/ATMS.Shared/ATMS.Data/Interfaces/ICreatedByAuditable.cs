namespace ATMS.Data.Interfaces;

public interface ICreatedByAuditable : IAuditable
{
    public Guid CreatedById { get; set; }
}
