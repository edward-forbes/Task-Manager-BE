using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Tasks.AnyAsync(cancellationToken)) return;

        db.Tasks.AddRange(
            new WorkTask
            {
                Title = "Connect the real PostgreSQL store",
                Description = "Persist task manager data in PostgreSQL.",
                Priority = TaskPriority.High,
                ClientName = "Internal",
                Tags = ["database", "gitops"]
            },
            new WorkTask
            {
                Title = "Create Grafana dashboard",
                Description = "Visualize API request rate, task operations and latency.",
                Status = TaskState.InProgress,
                Priority = TaskPriority.Medium,
                TimeEstimateHours = 3,
                Tags = ["observability"]
            },
            new WorkTask
            {
                Title = "Review Kubernetes probes",
                Status = TaskState.PendingReview,
                Priority = TaskPriority.Low,
                TimeEstimateHours = 1,
                Tags = ["kubernetes"]
            });

        await db.SaveChangesAsync(cancellationToken);
    }
}
