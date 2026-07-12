using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Criteria.Organizations;

public class OrganizationFilter : ACriteria<Organization>
{
    public string? Search { get; init; }
    
    public DateTime? CreatedFrom { get; init; }
    
    public DateTime? CreatedTo { get; init; }
    
    public string? SortBy { get; init; }
    
    public SortDirectionEnum SortDirection { get; init; } = SortDirectionEnum.Asc;
    
    public override IQueryable<Organization> Apply(IQueryable<Organization> query)
    {
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            
            query = query.Where(o =>
                EF.Functions.ILike(o.Title, $"%{search}%") ||
                EF.Functions.ILike(o.Voen, $"%{search}%"));
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
                "createdat" => SortDirection == SortDirectionEnum.Asc
                    ? query.OrderBy(u => u.CreatedAt)
                    : query.OrderByDescending(u => u.CreatedAt),
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