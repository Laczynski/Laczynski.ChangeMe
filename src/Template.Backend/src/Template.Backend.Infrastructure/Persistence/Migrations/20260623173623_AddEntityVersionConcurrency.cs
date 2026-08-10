using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Template.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityVersionConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "template_backend",
                table: "users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "template_backend",
                table: "user_sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "template_backend",
                table: "roles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "template_backend",
                table: "notifications",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "template_backend",
                table: "issues",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "template_backend",
                table: "issue_watchers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "template_backend",
                table: "issue_history_entries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "template_backend",
                table: "issue_comments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "template_backend",
                table: "issue_acceptance_criteria",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                schema: "template_backend",
                table: "attachments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                schema: "template_backend",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "template_backend",
                table: "user_sessions");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "template_backend",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "template_backend",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "template_backend",
                table: "issues");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "template_backend",
                table: "issue_watchers");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "template_backend",
                table: "issue_history_entries");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "template_backend",
                table: "issue_comments");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "template_backend",
                table: "issue_acceptance_criteria");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "template_backend",
                table: "attachments");
        }
    }
}
