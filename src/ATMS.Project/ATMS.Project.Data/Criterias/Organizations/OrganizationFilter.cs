using ATMS.Data.Criterias;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criterias.Organizations;

public class OrganizationFilter : ACriteria<Organization>
{
    public string? Title { get; init; }
    public string? Voen { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public string? SortBy { get; init; }
    public SortDirectionEnum SortDirection { get; init; } = SortDirectionEnum.Asc;
    
    public override IQueryable<Organization> Apply(IQueryable<Organization> query)
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            var title = Title.Trim().ToLower();
            query = query.Where(u => u.Title.ToLower().StartsWith(title));
        }
        
        if (!string.IsNullOrWhiteSpace(Voen))
        {
            var voen = Voen.Trim().ToLower();
            query = query.Where(u => u.Voen.ToLower().StartsWith(voen));
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
        
        if (!string.IsNullOrWhiteSpace(SortBy))
        {
            query = SortBy.ToLower() switch
            {
                "title"      => SortDirection == SortDirectionEnum.Asc
                    ? query.OrderBy(u => u.Title)
                    : query.OrderByDescending(u => u.Title),
                "voen"   => SortDirection == SortDirectionEnum.Asc
                    ? query.OrderBy(u => u.Voen)
                    : query.OrderByDescending(u => u.Voen),
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