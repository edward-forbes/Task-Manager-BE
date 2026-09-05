using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public interface ITaskStore
{
    Task<IReadOnlyCollection<WorkTask>> GetAllAsync(CancellationToken cancellationToken);
    Task<WorkTask?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<WorkTask> AddAsync(WorkTask task, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class PostgresTaskStore(AppDbContext db) : ITaskStore
{
    public async Task<IReadOnlyCollection<WorkTask>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Tasks.AsNoTracking().Include(x => x.Comments)
            .OrderBy(x => x.CreatedAt).ToArrayAsync(cancellationToken);

    public Task<WorkTask?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Tasks.Include(x => x.Comments).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<WorkTask> AddAsync(WorkTask task, CancellationToken cancellationToken)
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await db.SaveChangesAsync(cancellationToken);

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await db.Tasks.Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken);
        return deleted > 0;
    }
}
