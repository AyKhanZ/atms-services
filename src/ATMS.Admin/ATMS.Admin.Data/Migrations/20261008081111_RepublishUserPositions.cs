using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMS.Admin.Data.Migrations
{
    /// <inheritdoc />
    public partial class RepublishUserPositions : Migration
    {
        // Project started keeping the position only now, so its copy is empty for everyone who filled it
        // in before. One user updated event per such user fills it; the outbox sends them as usual.
        // The payload is UserUpdatedEvent as System.Text.Json writes it: property names as declared.
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
                    'user.updated',
                    'ATMS.Contracts.Events.Users.UserUpdatedEvent',
                    json_build_object(
                        'Id', u."Id",
                        'Name', u."Name",
                        'Surname', u."Surname",
                        'AvatarPath', u."AvatarPath",
                        'HasCompletedOnboarding', u."HasCompletedOnboarding",
                        'Position', u."Position")::text,
                    1,
                    0,
                    now(),
                    now()
                FROM "Users" u
                WHERE u."Position" IS NOT NULL AND u."Position" <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sent events cannot be taken back, and Project keeping the position does no harm.
        }
    }
}
