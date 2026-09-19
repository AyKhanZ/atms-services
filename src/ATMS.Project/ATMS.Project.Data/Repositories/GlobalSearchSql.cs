using System.Runtime.CompilerServices;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Models.Search;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

/// <summary>
/// SQL shared by search and by the recent items: who may see what, and how a found row is turned
/// into what the list draws. Kept in one place so the two repositories cannot disagree about
/// access.
/// </summary>
internal static class GlobalSearchSql
{
    // Raw SQL bypasses EF query filters. Keep deletion and project boundaries explicit here.
    // {0} is "the caller is a super administrator", {1} is the caller's user id.
    // language=sql
    public const string AccessScope = """
        WITH accessible_projects AS NOT MATERIALIZED (
            SELECT p."Id", p."Code", p."Title", p."ProjectStatusId", p."CreatedAt"
            FROM "Projects" p
            WHERE NOT p."IsDeleted"
              AND ({0} OR EXISTS (
                  SELECT 1 FROM "ProjectParticipants" member
                  WHERE member."WorkProjectId" = p."Id"
                    AND member."UserId" = {1} AND NOT member."IsDeleted"))
        ), visible_tickets AS NOT MATERIALIZED (
            SELECT t."Id", t."Code", t."Title", t."WorkProjectId", t."WorkTicketStatusId",
                   t."AssigneeId", t."WorkGroupId", t."CreatedAt", m."ParentWorkGroupId" AS "GroupId"
            FROM "Tickets" t
            JOIN accessible_projects p ON p."Id" = t."WorkProjectId"
            JOIN "ProjectGroups" m ON m."Id" = t."WorkGroupId"
                AND m."WorkProjectId" = p."Id" AND NOT m."IsDeleted"
            JOIN "ProjectGroups" g ON g."Id" = m."ParentWorkGroupId"
                AND g."WorkProjectId" = p."Id" AND NOT g."IsDeleted"
            WHERE NOT t."IsDeleted"
        )
        """;

    // Enrich only the bounded rows. No entity materialization, tracking or Include is needed.
    // {2} is the language the status names are shown in.
    // language=sql
    public const string Projection = """
        SELECT s."ItemType", s."Id", s."Code", s."Title", s."ProjectId",
               p."Code" AS "ProjectCode", p."Title" AS "ProjectTitle", s."StatusId",
               COALESCE(ps."Code", ts."Code", ws."Code") AS "StatusCode",
               COALESCE(pst."Name", tst."Name", wst."Name", ps."Code", ts."Code", ws."Code") AS "StatusName",
               u."Id" AS "AssigneeId", u."Name" AS "AssigneeName", u."Surname" AS "AssigneeSurname",
               u."Email" AS "AssigneeEmail", u."AvatarPath" AS "AssigneeAvatarPath",
               s."GroupId", g."Title" AS "GroupTitle", s."MilestoneId", m."Title" AS "MilestoneTitle",
               s."TicketId", ticket."Code" AS "TicketCode", ticket."Title" AS "TicketTitle",
               s."ParentTaskId", parent."Code" AS "ParentTaskCode", parent."Title" AS "ParentTaskTitle",
               s."CreatedAt", s."OpenedAt"
        FROM selected s
        JOIN accessible_projects p ON p."Id" = s."ProjectId"
        LEFT JOIN "ProjectStatuses" ps ON s."ItemType" = 1 AND ps."Id" = s."StatusId"
        LEFT JOIN "ProjectStatusTranslation" pst ON pst."ProjectStatusId" = ps."Id" AND pst."Language" = {2}
        LEFT JOIN "WorkTicketStatuses" ts ON s."ItemType" = 2 AND ts."Id" = s."StatusId"
        LEFT JOIN "WorkTicketStatusTranslation" tst ON tst."WorkTicketStatusId" = ts."Id" AND tst."Language" = {2}
        LEFT JOIN "WorkTaskStatuses" ws ON s."ItemType" IN (3, 4) AND ws."Id" = s."StatusId"
        LEFT JOIN "WorkTaskStatusTranslation" wst ON wst."WorkTaskStatusId" = ws."Id" AND wst."Language" = {2}
        LEFT JOIN "ProjectParticipants" assignee ON assignee."Id" = s."AssigneeId"
            AND assignee."WorkProjectId" = s."ProjectId" AND NOT assignee."IsDeleted"
        LEFT JOIN "Users" u ON u."Id" = assignee."UserId" AND NOT u."IsDeleted"
        LEFT JOIN "ProjectGroups" g ON g."Id" = s."GroupId" AND NOT g."IsDeleted"
        LEFT JOIN "ProjectGroups" m ON m."Id" = s."MilestoneId" AND NOT m."IsDeleted"
        LEFT JOIN "Tickets" ticket ON ticket."Id" = s."TicketId" AND NOT ticket."IsDeleted"
        LEFT JOIN "Tasks" parent ON parent."Id" = s."ParentTaskId" AND NOT parent."IsDeleted"
        """;

    /// <summary>
    /// Runs the access scope, the rows the caller picked as <c>selected</c>, and the projection
    /// that turns them into list rows. The first three arguments are always the super administrator
    /// flag, the user id and the language.
    /// </summary>
    public static Task<GlobalSearchRow[]> QueryAsync(
        ProjectDbContext context,
        string selected,
        string order,
        object?[] arguments,
        CancellationToken cancellationToken)
    {
        var query = FormattableStringFactory.Create(
            $"{AccessScope}\n{selected}\n{Projection}\n{order}", arguments);

        return context.Database.SqlQuery<GlobalSearchRow>(query)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);
    }
}
