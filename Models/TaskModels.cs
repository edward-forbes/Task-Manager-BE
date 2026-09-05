namespace TaskManager.Api.Models;

public enum TaskState { New, InProgress, OnHold, PendingReview, Complete }
public enum TaskPriority { Low, Medium, High, Critical }

public sealed class TaskComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkTaskId { get; set; }
    public string Author { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class WorkTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public string Description { get; set; } = "";
    public TaskState Status { get; set; } = TaskState.New;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public decimal? TimeEstimateHours { get; set; }
    public string ClientName { get; set; } = "";
    public string Assignee { get; set; } = "";
    public DateOnly? DueDate { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<TaskComment> Comments { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed record LoginRequest(string Username, string Password);
public sealed record LoginResponse(string Token, string Username, DateTimeOffset ExpiresAt);
public sealed record CreateTaskRequest(string Title, string? Description, TaskPriority Priority,
    decimal? TimeEstimateHours, string? ClientName, string? Assignee, DateOnly? DueDate, List<string>? Tags);
public sealed record UpdateTaskRequest(string Title, string? Description, TaskState Status, TaskPriority Priority,
    decimal? TimeEstimateHours, string? ClientName, string? Assignee, DateOnly? DueDate, List<string>? Tags);
public sealed record ChangeStatusRequest(TaskState Status);
public sealed record AddCommentRequest(string Body);
