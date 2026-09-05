using System.Collections.Concurrent;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public interface ITaskStore
{
    IReadOnlyCollection<WorkTask> GetAll();
    WorkTask? Get(Guid id);
    WorkTask Add(WorkTask task);
    bool Delete(Guid id);
}

public sealed class InMemoryTaskStore : ITaskStore
{
    private readonly ConcurrentDictionary<Guid, WorkTask> _tasks = new();

    public InMemoryTaskStore()
    {
        foreach (var task in SeedData()) _tasks[task.Id] = task;
    }

    public IReadOnlyCollection<WorkTask> GetAll() => _tasks.Values.OrderBy(x => x.CreatedAt).ToArray();
    public WorkTask? Get(Guid id) => _tasks.GetValueOrDefault(id);
    public WorkTask Add(WorkTask task) { _tasks[task.Id] = task; return task; }
    public bool Delete(Guid id) => _tasks.TryRemove(id, out _);

    private static IEnumerable<WorkTask> SeedData()
    {
        yield return new WorkTask { Title = "Connect the real PostgreSQL store", Description = "Replace the in-memory repository in the next milestone.", Priority = TaskPriority.High, ClientName = "Internal", Tags = ["database", "gitops"] };
        yield return new WorkTask { Title = "Create Grafana dashboard", Description = "Visualize API request rate, task operations and latency.", Status = TaskState.InProgress, Priority = TaskPriority.Medium, TimeEstimateHours = 3, Tags = ["observability"] };
        yield return new WorkTask { Title = "Review Kubernetes probes", Status = TaskState.PendingReview, Priority = TaskPriority.Low, TimeEstimateHours = 1, Tags = ["kubernetes"] };
    }
}
