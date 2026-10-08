using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Models.Search;
using ATMS.Project.Data.Repositories.Interfaces;

namespace ATMS.Project.Data.Repositories;

public sealed class GlobalSearchRepository(ProjectDbContext context) : IGlobalSearchRepository
{
    private const int MinimumTitleLength = 3;
    private const int MaximumQueryLength = 100;

    // language=sql
    private const string SearchOrder = """ORDER BY s."CreatedAt" @@dir@@, s."Id" @@dir@@""";

    // newest first like every other list; ordering by code would show the oldest first and #100 before #99
    // language=sql
    private const string BranchOrder = """ORDER BY @@alias@@."CreatedAt" @@dir@@, @@alias@@."Id" @@dir@@""";

    private sealed record SearchBranch(GlobalSearchItemTypeEnum Type, string Alias, string Sql);

    // one branch per type with its own limit, so the db can stop reading early
    private static readonly SearchBranch[] Branches =
    [
        // language=sql
        new(GlobalSearchItemTypeEnum.Project, "p", """
        (SELECT 1 AS "ItemType", p."Id", p."Code", p."Title", p."Id" AS "ProjectId",
                p."ProjectStatusId" AS "StatusId", NULL::uuid AS "AssigneeId",
                NULL::uuid AS "GroupId", NULL::uuid AS "MilestoneId",
                NULL::uuid AS "TicketId", NULL::uuid AS "ParentTaskId",
                p."CreatedAt", NULL::timestamptz AS "OpenedAt"
         FROM accessible_projects p
         WHERE @@where@@
         @@order@@ LIMIT @@limit@@)
        """),
        // language=sql
        new(GlobalSearchItemTypeEnum.Ticket, "t", """
        (SELECT 2 AS "ItemType", t."Id", t."Code", t."Title", t."WorkProjectId" AS "ProjectId",
                t."WorkTicketStatusId" AS "StatusId", t."AssigneeId",
                t."GroupId", t."WorkGroupId" AS "MilestoneId",
                NULL::uuid AS "TicketId", NULL::uuid AS "ParentTaskId",
                t."CreatedAt", NULL::timestamptz AS "OpenedAt"
         FROM visible_tickets t
         WHERE @@where@@
         @@order@@ LIMIT @@limit@@)
        """),
        // language=sql
        new(GlobalSearchItemTypeEnum.Task, "t", """
        (SELECT 3 AS "ItemType", t."Id", t."Code", t."Title", t."WorkProjectId" AS "ProjectId",
                t."StatusId", t."AssigneeId",
                ticket."GroupId", ticket."WorkGroupId" AS "MilestoneId",
                ticket."Id" AS "TicketId", NULL::uuid AS "ParentTaskId",
                t."CreatedAt", NULL::timestamptz AS "OpenedAt"
         FROM "Tasks" t
         JOIN visible_tickets ticket ON ticket."Id" = t."WorkTicketId"
             AND ticket."WorkProjectId" = t."WorkProjectId"
         WHERE NOT t."IsDeleted" AND t."ParentWorkTaskId" IS NULL AND @@where@@
         @@order@@ LIMIT @@limit@@)
        """),
        // language=sql
        new(GlobalSearchItemTypeEnum.Subtask, "t", """
        (SELECT 4 AS "ItemType", t."Id", t."Code", t."Title", t."WorkProjectId" AS "ProjectId",
                t."StatusId", t."AssigneeId",
                ticket."GroupId", ticket."WorkGroupId" AS "MilestoneId",
                ticket."Id" AS "TicketId", parent."Id" AS "ParentTaskId",
                t."CreatedAt", NULL::timestamptz AS "OpenedAt"
         FROM "Tasks" t
         JOIN visible_tickets ticket ON ticket."Id" = t."WorkTicketId"
             AND ticket."WorkProjectId" = t."WorkProjectId"
         JOIN "Tasks" parent ON parent."Id" = t."ParentWorkTaskId"
             AND parent."WorkProjectId" = t."WorkProjectId"
             AND parent."WorkTicketId" = t."WorkTicketId"
             AND NOT parent."IsDeleted" AND parent."ParentWorkTaskId" IS NULL
         WHERE NOT t."IsDeleted" AND @@where@@
         @@order@@ LIMIT @@limit@@)
        """)
    ];

    public Task<GlobalSearchRow[]> SearchAsync(Guid userId, bool isSuperAdmin, string search, int take, string language, CancellationToken cancellationToken)
    {
        var predicate = BuildPredicate(search, firstIndex: 3);
        if (predicate.Sql is null)
        {
            return Task.FromResult(Array.Empty<GlobalSearchRow>());
        }

        // {0} super admin, {1} user, {2} language, then the predicate values, then the limit
        // limit is take + 1: the extra row tells if there is a next page
        object?[] arguments = [isSuperAdmin, userId, language, .. predicate.Arguments, take + 1];
        var limit = "{" + (arguments.Length - 1) + "}";
        const SortDirectionEnum newest = SortDirectionEnum.Desc;
        var branches = string.Join(
            "\n    UNION ALL\n",
            Branches.Select(branch => Compose(branch, predicate.Sql, limit, newest)));

        return GlobalSearchSql.QueryAsync(
            context, $", selected AS (\n{branches}\n)", Order(SearchOrder, newest), arguments, cancellationToken);
    }

    public Task<GlobalSearchRow[]> SearchPageAsync(
        Guid userId,
        bool isSuperAdmin,
        string search,
        GlobalSearchItemTypeEnum itemType,
        KeysetCursor? cursor,
        SortDirectionEnum sortDirection,
        int pageSize,
        string language,
        CancellationToken cancellationToken)
    {
        var predicate = BuildPredicate(search, firstIndex: 3);
        if (predicate.Sql is null)
        {
            return Task.FromResult(Array.Empty<GlobalSearchRow>());
        }

        var arguments = new List<object?> { isSuperAdmin, userId, language };
        arguments.AddRange(predicate.Arguments);

        var where = predicate.Sql;
        if (cursor is not null)
        {
            var after = sortDirection == SortDirectionEnum.Asc ? ">" : "<";
            where += " AND (@@alias@@.\"CreatedAt\", @@alias@@.\"Id\") " + after + " ({"
                + arguments.Count + "}, {" + (arguments.Count + 1) + "})";
            arguments.Add(cursor.KeyAs<DateTime>());
            arguments.Add(cursor.Id);
        }

        var limit = "{" + arguments.Count + "}";
        arguments.Add(pageSize + 1);

        var branch = Compose(Branches.Single(value => value.Type == itemType), where, limit, sortDirection);

        return GlobalSearchSql.QueryAsync(
            context,
            $", selected AS (\n{branch}\n)",
            Order(SearchOrder, sortDirection),
            [.. arguments],
            cancellationToken);
    }

    private static string Compose(SearchBranch branch, string where, string limit, SortDirectionEnum sortDirection)
    {
        return branch.Sql
            .Replace("@@where@@", "(" + where.Replace("@@alias@@", branch.Alias) + ")")
            .Replace("@@order@@", Order(BranchOrder.Replace("@@alias@@", branch.Alias), sortDirection))
            .Replace("@@limit@@", limit);
    }

    // branch limits and the outer order must agree
    private static string Order(string template, SortDirectionEnum sortDirection) =>
        template.Replace("@@dir@@", sortDirection == SortDirectionEnum.Asc ? "ASC" : "DESC");

    // code is matched exactly and title by "contains", so both sides of the OR use an index
    // Sql is null when there is nothing to search for
    private static (string? Sql, object?[] Arguments) BuildPredicate(string search, int firstIndex)
    {
        var text = search.Trim();
        var code = text.TrimStart('#').Trim();
        var parts = new List<string>();
        var arguments = new List<object?>();

        if (text.Length <= MaximumQueryLength && code.Length > 0 && code.All(char.IsDigit))
        {
            parts.Add("@@alias@@.\"Code\" = {" + (firstIndex + arguments.Count) + "}");
            arguments.Add(code);
        }

        // "#" means searching by code; under 3 chars the trigram index doesn't help
        if (text.Length is >= MinimumTitleLength and <= MaximumQueryLength && !text.StartsWith('#'))
        {
            parts.Add("@@alias@@.\"Title\" ILIKE {" + (firstIndex + arguments.Count) + "} ESCAPE '\\'");
            arguments.Add($"%{EscapeLike(text)}%");
        }

        return parts.Count switch
        {
            0 => (null, []),
            1 => (parts[0], arguments.ToArray()),
            _ => ($"({string.Join(" OR ", parts)})", arguments.ToArray())
        };
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
