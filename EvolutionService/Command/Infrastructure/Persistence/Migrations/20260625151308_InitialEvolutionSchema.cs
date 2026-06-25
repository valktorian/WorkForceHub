using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EvolutionService.Command.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialEvolutionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "evolution_command");

            migrationBuilder.CreateTable(
                name: "job_movements",
                schema: "evolution_command",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousJobTitle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NewJobTitle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    PreviousDepartment = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NewDepartment = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_movements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "evolution_command",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rewards",
                schema: "evolution_command",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "salary_changes",
                schema: "evolution_command",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NewSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salary_changes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trainings",
                schema: "evolution_command",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificateUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trainings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_job_movements_EffectiveDate",
                schema: "evolution_command",
                table: "job_movements",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_job_movements_EmployeeId",
                schema: "evolution_command",
                table: "job_movements",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_PublishedAt",
                schema: "evolution_command",
                table: "outbox_messages",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_rewards_EmployeeId",
                schema: "evolution_command",
                table: "rewards",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_rewards_GrantedAt",
                schema: "evolution_command",
                table: "rewards",
                column: "GrantedAt");

            migrationBuilder.CreateIndex(
                name: "IX_salary_changes_EffectiveDate",
                schema: "evolution_command",
                table: "salary_changes",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_salary_changes_EmployeeId",
                schema: "evolution_command",
                table: "salary_changes",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_trainings_EmployeeId",
                schema: "evolution_command",
                table: "trainings",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_trainings_StartDate",
                schema: "evolution_command",
                table: "trainings",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_trainings_Status",
                schema: "evolution_command",
                table: "trainings",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_movements",
                schema: "evolution_command");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "evolution_command");

            migrationBuilder.DropTable(
                name: "rewards",
                schema: "evolution_command");

            migrationBuilder.DropTable(
                name: "salary_changes",
                schema: "evolution_command");

            migrationBuilder.DropTable(
                name: "trainings",
                schema: "evolution_command");
        }
    }
}
