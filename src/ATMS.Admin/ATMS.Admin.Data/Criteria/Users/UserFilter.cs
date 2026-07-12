using ATMS.Admin.Data.Entities;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Criteria.Users;

public class UserFilter : ACriteria<User>
{
    public string? Search { get; init; }
    
    public DateTime? CreatedFrom { get; init; }
    
    public DateTime? CreatedTo { get; init; }
    
    public int? UserStatusId { get; init; }
    
    public string? SortBy { get; init; }
    
    public SortDirectionEnum SortDirection { get; init; } = SortDirectionEnum.Asc;
    
    public override IQueryable<User> Apply(IQueryable<User> query)
    {
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            
            query = query.Where(u =>
                EF.Functions.ILike(u.Name, $"%{search}%") ||
                EF.Functions.ILike(u.Surname, $"%{search}%") ||
                EF.Functions.ILike(u.Email, $"%{search}%") ||
                EF.Functions.ILike(u.Position ?? string.Empty, $"%{search}%"));
        }
        
        if (CreatedFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(CreatedFrom.Value, DateTimeKind.Utc);
            query = query.Where(u => u.CreatedAt >= from);
        }

        if (CreatedTo.HasValue)
        {
            var to = DateTime.SpecifyKind(CreatedTo.Value, DateTimeKind.Utc);
            query = query.Where(u => u.CreatedAt <= to);
        }

        if (UserStatusId > 0)
        {
            query = query.Where(u => u.UserStatusId == UserStatusId.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(SortBy))
        {
            query = SortBy.ToLower() switch
            {
                "name"      => SortDirection == SortDirectionEnum.Asc
                    ? query.OrderBy(u => u.Name)
                    : query.OrderByDescending(u => u.Name),
                "surname"   => SortDirection == SortDirectionEnum.Asc
                    ? query.OrderBy(u => u.Surname)
                    : query.OrderByDescending(u => u.Surname),
                "email"     => SortDirection == SortDirectionEnum.Asc
                    ? query.OrderBy(u => u.Email)
                    : query.OrderByDescending(u => u.Email),
                "position" => SortDirection == SortDirectionEnum.Asc
                    ? query.OrderBy(u => u.Position)
                    : query.OrderByDescending(u => u.Position),
                "createdat" => SortDirection == SortDirectionEnum.Asc
                    ? query.OrderBy(u => u.CreatedAt)
                    : query.OrderByDescending(u => u.CreatedAt),
                "userstatus" => SortDirection == SortDirectionEnum.Asc
                    ? query.OrderBy(u => u.UserStatusId)
                    : query.OrderByDescending(u => u.UserStatusId),
                _           => query.OrderByDescending(u => u.CreatedAt)
            };
        }
        else
        {
            query = query.OrderByDescending(u => u.CreatedAt);
        }
        
        return query;
    }
}
