using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeService.Command.Infrastructure.Persistence.Migrations;

[DbContext(typeof(TimeCommandDbContext))]
[Migration("20260726000000_InitialTimeCommandSchema")]
public partial class InitialTimeCommandSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "time_command");

        migrationBuilder.CreateTable(
            name: "leave_balances",
            schema: "time_command",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                LeaveType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Available = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                Used = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                Pending = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_leave_balances", x => x.Id));

        migrationBuilder.CreateTable(
            name: "leave_requests",
            schema: "time_command",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                LeaveType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                DecisionComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                DecisionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_leave_requests", x => x.Id));

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            schema: "time_command",
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
            constraints: table => table.PrimaryKey("PK_outbox_messages", x => x.Id));

        migrationBuilder.CreateTable(
            name: "time_entries",
            schema: "time_command",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                WorkDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                StartTime = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                EndTime = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                Hours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                ProjectCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                TaskCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_time_entries", x => x.Id));

        migrationBuilder.CreateTable(
            name: "timesheets",
            schema: "time_command",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                TotalHours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ReviewComment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_timesheets", x => x.Id));

        migrationBuilder.CreateIndex("IX_leave_balances_EmployeeId_LeaveType", "leave_balances",
            new[] { "EmployeeId", "LeaveType" }, "time_command", unique: true);
        migrationBuilder.CreateIndex("IX_leave_requests_AccountId", "leave_requests", "AccountId", "time_command");
        migrationBuilder.CreateIndex("IX_leave_requests_EmployeeId", "leave_requests", "EmployeeId", "time_command");
        migrationBuilder.CreateIndex("IX_leave_requests_Status", "leave_requests", "Status", "time_command");
        migrationBuilder.CreateIndex("IX_outbox_messages_PublishedAt", "outbox_messages", "PublishedAt", "time_command");
        migrationBuilder.CreateIndex("IX_time_entries_AccountId", "time_entries", "AccountId", "time_command");
        migrationBuilder.CreateIndex("IX_time_entries_EmployeeId", "time_entries", "EmployeeId", "time_command");
        migrationBuilder.CreateIndex("IX_time_entries_WorkDate", "time_entries", "WorkDate", "time_command");
        migrationBuilder.CreateIndex("IX_timesheets_AccountId", "timesheets", "AccountId", "time_command");
        migrationBuilder.CreateIndex("IX_timesheets_EmployeeId", "timesheets", "EmployeeId", "time_command");
        migrationBuilder.CreateIndex("IX_timesheets_EmployeeId_PeriodStart_PeriodEnd", "timesheets",
            new[] { "EmployeeId", "PeriodStart", "PeriodEnd" }, "time_command", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("leave_balances", "time_command");
        migrationBuilder.DropTable("leave_requests", "time_command");
        migrationBuilder.DropTable("outbox_messages", "time_command");
        migrationBuilder.DropTable("time_entries", "time_command");
        migrationBuilder.DropTable("timesheets", "time_command");
    }
}
