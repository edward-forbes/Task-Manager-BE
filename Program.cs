using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using Serilog;
using Serilog.Formatting.Compact;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "task-manager-api")
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddHealthChecks();
    builder.Services.AddSingleton<ITaskStore, InMemoryTaskStore>();

    var jwtKey = builder.Configuration["Auth:JwtKey"] ?? "local-development-key-change-before-production-12345";
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = "task-manager-api", ValidAudience = "task-manager-ui",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        });
    builder.Services.AddAuthorization();
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
        .WithOrigins(builder.Configuration["FrontendOrigin"] ?? "http://localhost:3000")
        .AllowAnyHeader().AllowAnyMethod()));

    var app = builder.Build();
    app.UseSerilogRequestLogging();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseHttpMetrics();
    app.MapMetrics("/metrics");
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/ready");
    app.UseSwagger();
    app.UseSwaggerUI();

    var taskCreated = Metrics.CreateCounter("task_manager_tasks_created_total", "Tasks created", new CounterConfiguration { LabelNames = ["priority"] });
    var taskMoved = Metrics.CreateCounter("task_manager_task_status_changes_total", "Task status changes", new CounterConfiguration { LabelNames = ["from", "to"] });
    var taskDeleted = Metrics.CreateCounter("task_manager_tasks_deleted_total", "Tasks deleted");
    var commentsAdded = Metrics.CreateCounter("task_manager_comments_added_total", "Comments added");

    app.MapPost("/api/auth/login", (LoginRequest request, IConfiguration config, ILogger<Program> logger) =>
    {
        var validUser = config["Auth:Username"] ?? "admin";
        var validPassword = config["Auth:Password"] ?? "admin";
        if (request.Username != validUser || request.Password != validPassword)
        {
            logger.LogWarning("Login failed for {Username}", request.Username);
            return Results.Unauthorized();
        }

        var expires = DateTimeOffset.UtcNow.AddHours(8);
        var token = new JwtSecurityToken("task-manager-api", "task-manager-ui",
            [new Claim(ClaimTypes.Name, request.Username)], expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), SecurityAlgorithms.HmacSha256));
        logger.LogInformation("User {Username} logged in", request.Username);
        return Results.Ok(new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), request.Username, expires));
    }).AllowAnonymous().WithTags("Authentication");

    var tasks = app.MapGroup("/api/tasks").RequireAuthorization().WithTags("Tasks");
    tasks.MapGet("", (ITaskStore store) => Results.Ok(store.GetAll()));
    tasks.MapGet("/{id:guid}", (Guid id, ITaskStore store) => store.Get(id) is { } task ? Results.Ok(task) : Results.NotFound());
    tasks.MapPost("/", (CreateTaskRequest request, ITaskStore store, ClaimsPrincipal user, ILogger<Program> logger) =>
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return Results.BadRequest(new { error = "Title is required." });
        var task = new WorkTask { Title = request.Title.Trim(), Description = request.Description ?? "", Priority = request.Priority,
            TimeEstimateHours = request.TimeEstimateHours, ClientName = request.ClientName ?? "", Assignee = request.Assignee ?? "",
            DueDate = request.DueDate, Tags = request.Tags ?? [] };
        store.Add(task); taskCreated.WithLabels(task.Priority.ToString()).Inc();
        logger.LogInformation("Task {TaskId} created by {Username} with priority {Priority}", task.Id, user.Identity?.Name, task.Priority);
        return Results.Created($"/api/tasks/{task.Id}", task);
    });
    tasks.MapPut("/{id:guid}", (Guid id, UpdateTaskRequest request, ITaskStore store, ILogger<Program> logger) =>
    {
        var task = store.Get(id); if (task is null) return Results.NotFound();
        var oldStatus = task.Status;
        task.Title = request.Title; task.Description = request.Description ?? ""; task.Status = request.Status; task.Priority = request.Priority;
        task.TimeEstimateHours = request.TimeEstimateHours; task.ClientName = request.ClientName ?? ""; task.Assignee = request.Assignee ?? "";
        task.DueDate = request.DueDate; task.Tags = request.Tags ?? []; task.UpdatedAt = DateTimeOffset.UtcNow;
        if (oldStatus != task.Status) taskMoved.WithLabels(oldStatus.ToString(), task.Status.ToString()).Inc();
        logger.LogInformation("Task {TaskId} updated; status is {Status}", id, task.Status); return Results.Ok(task);
    });
    tasks.MapPatch("/{id:guid}/status", (Guid id, ChangeStatusRequest request, ITaskStore store, ILogger<Program> logger) =>
    {
        var task = store.Get(id); if (task is null) return Results.NotFound();
        var old = task.Status; task.Status = request.Status; task.UpdatedAt = DateTimeOffset.UtcNow;
        taskMoved.WithLabels(old.ToString(), task.Status.ToString()).Inc();
        logger.LogInformation("Task {TaskId} moved from {OldStatus} to {NewStatus}", id, old, task.Status); return Results.Ok(task);
    });
    tasks.MapPost("/{id:guid}/comments", (Guid id, AddCommentRequest request, ITaskStore store, ClaimsPrincipal user) =>
    {
        var task = store.Get(id); if (task is null) return Results.NotFound();
        if (string.IsNullOrWhiteSpace(request.Body)) return Results.BadRequest(new { error = "Comment cannot be empty." });
        task.Comments.Add(new TaskComment(Guid.NewGuid(), user.Identity?.Name ?? "unknown", request.Body.Trim(), DateTimeOffset.UtcNow));
        task.UpdatedAt = DateTimeOffset.UtcNow; commentsAdded.Inc(); return Results.Ok(task);
    });
    tasks.MapDelete("/{id:guid}", (Guid id, ITaskStore store) => { if (!store.Delete(id)) return Results.NotFound(); taskDeleted.Inc(); return Results.NoContent(); });

    app.Run();
}
catch (Exception ex) { Log.Fatal(ex, "Application terminated unexpectedly"); }
finally { await Log.CloseAndFlushAsync(); }

public partial class Program { }
