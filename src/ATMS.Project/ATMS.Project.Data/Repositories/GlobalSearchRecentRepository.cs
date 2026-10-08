using System.Runtime.CompilerServices;
using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Models.Search;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public sealed class GlobalSearchRecentRepository(ProjectDbContext context) : IGlobalSearchRecentRepository
{
    private const int RecentRetention = 20;
    private const int RecentTake = 5;

    // a constant item type per branch, so the planner drops the branches that can't match
    // language=sql
    private const string RecentBranches = """
        , recent AS (
            SELECT r."ItemType", r."ItemId", r."OpenedAt"
            FROM "GlobalSearchRecentItems" r
            WHERE r."UserId" = {1}
        ), selected AS (
            (SELECT 1 AS "ItemType", p."Id", p."Code", p."Title", p."Id" AS "ProjectId",
                    p."ProjectStatusId" AS "StatusId", NULL::uuid AS "AssigneeId",
                    NULL::uuid AS "GroupId", NULL::uuid AS "MilestoneId",
                    NULL::uuid AS "TicketId", NULL::uuid AS "ParentTaskId",
                    p."CreatedAt", r."OpenedAt"
             FROM recent r
             JOIN accessible_projects p ON p."Id" = r."ItemId"
             WHERE r."ItemType" = 1)
            UNION ALL
            (SELECT 2, t."Id", t."Code", t."Title", t."WorkProjectId", t."WorkTicketStatusId",
                    t."AssigneeId", t."GroupId", t."WorkGroupId", NULL::uuid, NULL::uuid,
                    t."CreatedAt", r."OpenedAt"
             FROM recent r
             JOIN visible_tickets t ON t."Id" = r."ItemId"
             WHERE r."ItemType" = 2)
            UNION ALL
            (SELECT 3, t."Id", t."Code", t."Title", t."WorkProjectId", t."StatusId",
                    t."AssigneeId", ticket."GroupId", ticket."WorkGroupId", ticket."Id", NULL::uuid,
                    t."CreatedAt", r."OpenedAt"
             FROM recent r
             JOIN "Tasks" t ON t."Id" = r."ItemId"
             JOIN visible_tickets ticket ON ticket."Id" = t."WorkTicketId"
                 AND ticket."WorkProjectId" = t."WorkProjectId"
             WHERE r."ItemType" = 3 AND NOT t."IsDeleted" AND t."ParentWorkTaskId" IS NULL)
            UNION ALL
            (SELECT 4, t."Id", t."Code", t."Title", t."WorkProjectId", t."StatusId",
                    t."AssigneeId", ticket."GroupId", ticket."WorkGroupId", ticket."Id", parent."Id",
                    t."CreatedAt", r."OpenedAt"
             FROM recent r
             JOIN "Tasks" t ON t."Id" = r."ItemId"
             JOIN visible_tickets ticket ON ticket."Id" = t."WorkTicketId"
                 AND ticket."WorkProjectId" = t."WorkProjectId"
             JOIN "Tasks" parent ON parent."Id" = t."ParentWorkTaskId"
                 AND parent."WorkProjectId" = t."WorkProjectId"
                 AND parent."WorkTicketId" = t."WorkTicketId"
                 AND NOT parent."IsDeleted" AND parent."ParentWorkTaskId" IS NULL
             WHERE r."ItemType" = 4 AND NOT t."IsDeleted")
        )
        """;

    public Task<GlobalSearchRow[]> GetRecentAsync(
        Guid userId, bool isSuperAdmin, string language, CancellationToken cancellationToken)
    {
        return GlobalSearchSql.QueryAsync(
            context,
            RecentBranches,
            $"ORDER BY s.\"OpenedAt\" DESC, s.\"ItemType\", s.\"Id\" LIMIT {RecentTake}",
            [isSuperAdmin, userId, language],
            cancellationToken);
    }

    public async Task<bool> RecordRecentAsync(
        Guid userId, bool isSuperAdmin, GlobalSearchItemTypeEnum itemType, Guid itemId, CancellationToken cancellationToken)
    {
        // no transaction or lock: it runs on every page open and the upsert is atomic, a few extra rows for a moment are fine
        var command = FormattableStringFactory.Create(
            $"{GlobalSearchSql.AccessScope}\n" + """
            INSERT INTO "GlobalSearchRecentItems" ("UserId", "ItemType", "ItemId", "OpenedAt")
            SELECT {1}, {2}, {3}, clock_timestamp()
            WHERE EXISTS (
            """ + $"\n{VisibilityCheck(itemType)}\n)\n" + """
            ON CONFLICT ("UserId", "ItemType", "ItemId")
            DO UPDATE SET "OpenedAt" = EXCLUDED."OpenedAt"
            """,
            isSuperAdmin, userId, (int)itemType, itemId);

        var affected = await context.Database.ExecuteSqlAsync(command, cancellationToken);
        if (affected == 0)
        {
            return false;
        }

        await context.Database.ExecuteSqlAsync($"""
            DELETE FROM "GlobalSearchRecentItems"
            WHERE "UserId" = {userId} AND ("ItemType", "ItemId") IN (
                SELECT "ItemType", "ItemId" FROM "GlobalSearchRecentItems"
                WHERE "UserId" = {userId}
                ORDER BY "OpenedAt" DESC, "ItemType", "ItemId" OFFSET {RecentRetention})
            """, cancellationToken);
        return true;
    }

    // {3} is the item id
    private static string VisibilityCheck(GlobalSearchItemTypeEnum itemType) => itemType switch
    {
        GlobalSearchItemTypeEnum.Project => """
            SELECT 1 FROM accessible_projects p WHERE p."Id" = {3}
            """,
        GlobalSearchItemTypeEnum.Ticket => """
            SELECT 1 FROM visible_tickets t WHERE t."Id" = {3}
            """,
        GlobalSearchItemTypeEnum.Task => """
            SELECT 1 FROM "Tasks" t
            JOIN visible_tickets ticket ON ticket."Id" = t."WorkTicketId"
                AND ticket."WorkProjectId" = t."WorkProjectId"
            WHERE t."Id" = {3} AND NOT t."IsDeleted" AND t."ParentWorkTaskId" IS NULL
            """,
        GlobalSearchItemTypeEnum.Subtask => """
            SELECT 1 FROM "Tasks" t
            JOIN visible_tickets ticket ON ticket."Id" = t."WorkTicketId"
                AND ticket."WorkProjectId" = t."WorkProjectId"
            JOIN "Tasks" parent ON parent."Id" = t."ParentWorkTaskId"
                AND parent."WorkProjectId" = t."WorkProjectId"
                AND parent."WorkTicketId" = t."WorkTicketId"
                AND NOT parent."IsDeleted" AND parent."ParentWorkTaskId" IS NULL
            WHERE t."Id" = {3} AND NOT t."IsDeleted"
            """,
        _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
    };
}
