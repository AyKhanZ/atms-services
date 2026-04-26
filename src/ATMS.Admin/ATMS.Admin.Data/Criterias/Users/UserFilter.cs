using ATMS.Admin.Data.Entities;
using ATMS.Data.Criterias;
using ATMS.Data.Enums;

namespace ATMS.Admin.Data.Criterias.Users;

public class UserFilter : ACriteria<User>
{
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public string? Email { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public int? UserStatusId { get; init; }
    public string? SortBy { get; init; }
    public SortDirectionEnum SortDirection { get; init; } = SortDirectionEnum.Asc;
    
    public override IQueryable<User> Apply(IQueryable<User> query)
    {
        if (!string.IsNullOrWhiteSpace(Name))
        {
            var name = Name.Trim().ToLower();
            query = query.Where(u => u.Name.ToLower().StartsWith(name));
        }
        
        if (!string.IsNullOrWhiteSpace(Surname))
        {
            var surname = Surname.Trim().ToLower();
            query = query.Where(u => u.Surname.ToLower().StartsWith(surname));
        }

        if (!string.IsNullOrWhiteSpace(Email))
        {
            var email = Email.Trim().ToLower();
            query = query.Where(u => u.Email.ToLower().StartsWith(email));
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