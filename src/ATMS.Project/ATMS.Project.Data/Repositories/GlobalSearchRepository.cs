using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Models.Search;
using ATMS.Project.Data.Repositories.Interfaces;

namespace ATMS.Project.Data.Repositories;

/// <summary>Finds projects, tickets, tasks and subtasks by code or title across every project the caller can see.</summary>
public class GlobalSearchRepository(ProjectDbContext context) : IGlobalSearchRepository
{
    private const int MinimumTitleLength = 3;
    private const int MaximumQueryLength = 100;

    // language=sql
    private const string SearchOrder = """ORDER BY s."CreatedAt" @@dir@@, s."Id" @@dir@@""";

    /// <summary>
    /// Newest first, the same order every other list in the product uses, and the order a cursor
    /// can page through. Ordering by code would be wrong twice over: it shows the oldest items
    /// first, and the column holds digits as text, so #100 would sort before #99.
    /// </summary>
    // language=sql
    private const string BranchOrder = """ORDER BY @@alias@@."CreatedAt" @@dir@@, @@alias@@."Id" @@dir@@""";

    private sealed record SearchBranch(GlobalSearchItemType Type, string Alias, string Sql);

    /// <summary>
    /// One branch per type, each with its own limit. The limit is what lets the database stop
    /// reading: a shared window function or a total count would force it to build every match
    /// first, and a short query matches a large part of the table.
    /// </summary>
    private static readonly SearchBranch[] Branches =
    [
        // language=sql
        new(GlobalSearchItemType.Project, "p", """
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
        new(GlobalSearchItemType.Ticket, "t", """
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
        new(GlobalSearchItemType.Task, "t", """
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
        new(GlobalSearchItemType.Subtask, "t", """
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

        // {0} super admin, {1} user, {2} language, then the predicate values, then the limit.
        // The limit is one over what was asked for: that extra row is the "is there more"
        // answer, and it is the only row the database produces beyond the page.
        object?[] arguments = [isSuperAdmin, userId, language, .. predicate.Arguments, take + 1];
        var limit = "{" + (arguments.Length - 1) + "}";
        // The palette always shows the newest first; only the page lets the reader turn it round.
        const SortDirectionEnum newest = SortDirectionEnum.Desc;
        var branches = string.Join(
            "\n    UNION ALL\n",
            Branches.Select(branch => Compose(branch, predicate.Sql, limit, newest)));

        return GlobalSearchSql.QueryAsync(
            context, $", selected AS (\n{branches}\n)", Order(SearchOrder, newest), arguments, cancellationToken);
    }

    /// <summary>
    /// The "show all" page: one type, paged by the same keyset cursor the other lists use, so a
    /// long result list is walked in order without an offset and without a total count.
    /// </summary>
    public Task<GlobalSearchRow[]> SearchPageAsync(
        Guid userId,
        bool isSuperAdmin,
        string search,
        GlobalSearchItemType itemType,
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

    /// <summary>The branch limits and the outer order have to agree, or the page would be cut
    /// from one end and read from the other.</summary>
    private static string Order(string template, SortDirectionEnum sortDirection) =>
        template.Replace("@@dir@@", sortDirection == SortDirectionEnum.Asc ? "ASC" : "DESC");

    /// <summary>
    /// A code is matched exactly and a title by "contains", so both halves can use an index and
    /// the OR between them stays a cheap combination of two index scans. A code prefix could not:
    /// btree does not serve LIKE under this collation, and one unindexable half of an OR makes the
    /// planner read the whole table, which costs the title index as well. Sql is null when the
    /// text gives the database nothing to look for.
    /// </summary>
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

        // A leading # means the person is naming a code, so the title half is off. Below three
        // characters the trigram index cannot help and the search would read the whole table.
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
