using ATMS.Admin.Data.Entities;
using ATMS.Data.Criterias;

namespace ATMS.Admin.Data.Criterias.Users;

public class UserFilter : ACriteria<User>
{
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public string? Email { get; init; }
    public int? UserStatusId { get; init; }
    
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
        
        if (!string.IsNullOrWhiteSpace(Email))
        {
            var email = Email.Trim().ToLower();
            query = query.Where(u => u.Email.ToLower().StartsWith(email));
        }
        
        if (UserStatusId > 0)
        {
            query = query.Where(u => u.UserStatusId == UserStatusId.Value);
        }
        
        return query;
    }
}