using ATMS.Data.Enums;

namespace ATMS.Project.Data.Services.Interfaces;

public interface IHistoryFieldMap
{
    IReadOnlyCollection<Type> EntityTypes { get; }

    bool TryGetField(Type entityType, string propertyName, out HistoryFieldEnum field);

    bool IsIgnored(Type entityType, string propertyName);
}
