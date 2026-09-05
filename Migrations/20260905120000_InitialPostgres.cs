using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TaskManager.Api.Data;

#nullable disable

namespace TaskManager.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260905120000_InitialPostgres")]
public partial class InitialPostgres : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tasks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                Priority = table.Column<int>(type: "integer", nullable: false),
                TimeEstimateHours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                ClientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Assignee = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                Tags = table.Column<List<string>>(type: "text[]", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_tasks", x => x.Id));

        migrationBuilder.CreateTable(
            name: "task_comments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                WorkTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                Author = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Body = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_task_comments", x => x.Id);
                table.ForeignKey(
                    name: "FK_task_comments_tasks_WorkTaskId",
                    column: x => x.WorkTaskId,
                    principalTable: "tasks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_task_comments_WorkTaskId", table: "task_comments", column: "WorkTaskId");
        migrationBuilder.CreateIndex(name: "IX_tasks_Priority", table: "tasks", column: "Priority");
        migrationBuilder.CreateIndex(name: "IX_tasks_Status", table: "tasks", column: "Status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "task_comments");
        migrationBuilder.DropTable(name: "tasks");
    }
}
