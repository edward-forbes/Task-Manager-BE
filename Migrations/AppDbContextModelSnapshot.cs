using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TaskManager.Api.Data;

#nullable disable

namespace TaskManager.Api.Migrations;

[DbContext(typeof(AppDbContext))]
public partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "9.0.8")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);
        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("TaskManager.Api.Models.TaskComment", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid");
            b.Property<string>("Author").IsRequired().HasMaxLength(100).HasColumnType("character varying(100)");
            b.Property<string>("Body").IsRequired().HasMaxLength(5000).HasColumnType("character varying(5000)");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<Guid>("WorkTaskId").HasColumnType("uuid");
            b.HasKey("Id");
            b.HasIndex("WorkTaskId");
            b.ToTable("task_comments");
        });

        modelBuilder.Entity("TaskManager.Api.Models.WorkTask", b =>
        {
            b.Property<Guid>("Id").HasColumnType("uuid");
            b.Property<string>("Assignee").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)");
            b.Property<string>("ClientName").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)");
            b.Property<DateTimeOffset>("CreatedAt").HasColumnType("timestamp with time zone");
            b.Property<string>("Description").IsRequired().HasMaxLength(5000).HasColumnType("character varying(5000)");
            b.Property<DateOnly?>("DueDate").HasColumnType("date");
            b.Property<int>("Priority").HasColumnType("integer");
            b.Property<int>("Status").HasColumnType("integer");
            b.Property<List<string>>("Tags").IsRequired().HasColumnType("text[]");
            b.Property<decimal?>("TimeEstimateHours").HasPrecision(8, 2).HasColumnType("numeric(8,2)");
            b.Property<string>("Title").IsRequired().HasMaxLength(200).HasColumnType("character varying(200)");
            b.Property<DateTimeOffset>("UpdatedAt").HasColumnType("timestamp with time zone");
            b.HasKey("Id");
            b.HasIndex("Priority");
            b.HasIndex("Status");
            b.ToTable("tasks");
        });

        modelBuilder.Entity("TaskManager.Api.Models.TaskComment", b =>
        {
            b.HasOne("TaskManager.Api.Models.WorkTask", null)
                .WithMany("Comments")
                .HasForeignKey("WorkTaskId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("TaskManager.Api.Models.WorkTask", b => b.Navigation("Comments"));
#pragma warning restore 612, 618
    }
}
