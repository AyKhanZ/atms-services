using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class PublishInactiveUserStatuses : Migration
    {
        // Project started keeping IsActive only now, so its copy is true for everyone, including people
        // who are already inactive. One status event per inactive user fills it; the outbox sends them as usual.
        // The payload is UserStatusChangedEvent as System.Text.Json writes it: property names as declared.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "OutboxMessages"
                    ("Id", "Exchange", "RoutingKey", "MessageType", "Payload", "Status", "AttemptCount",
                     "CreatedAt", "NextAttemptAt")
                SELECT
                    gen_random_uuid(),
                    'atms.user.events',
                    'user.status.changed',
                    'ATMS.Contracts.Events.Users.UserStatusChangedEvent',
                    json_build_object(
                        'Id', u."Id",
                        'IsActive', false)::text,
                    1,
                    0,
                    now(),
                    now()
                FROM "Users" u
                WHERE u."UserStatusId" = 2;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sent events cannot be taken back, and Project keeping IsActive does no harm.
        }
    }
}
