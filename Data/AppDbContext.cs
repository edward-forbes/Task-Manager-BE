using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WorkTask> Tasks => Set<WorkTask>();
    public DbSet<TaskComment> Comments => Set<TaskComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkTask>(task =>
        {
            task.ToTable("tasks");
            task.HasKey(x => x.Id);
            task.Property(x => x.Title).HasMaxLength(200).IsRequired();
            task.Property(x => x.Description).HasMaxLength(5000);
            task.Property(x => x.ClientName).HasMaxLength(200);
            task.Property(x => x.Assignee).HasMaxLength(200);
            task.Property(x => x.TimeEstimateHours).HasPrecision(8, 2);
            task.Property(x => x.Tags).HasColumnType("text[]");
            task.HasMany(x => x.Comments)
                .WithOne()
                .HasForeignKey(x => x.WorkTaskId)
                .OnDelete(DeleteBehavior.Cascade);
            task.HasIndex(x => x.Status);
            task.HasIndex(x => x.Priority);
        });

        modelBuilder.Entity<TaskComment>(comment =>
        {
            comment.ToTable("task_comments");
            comment.HasKey(x => x.Id);
            comment.Property(x => x.Author).HasMaxLength(100).IsRequired();
            comment.Property(x => x.Body).HasMaxLength(5000).IsRequired();
            comment.HasIndex(x => x.WorkTaskId);
        });
    }
}
