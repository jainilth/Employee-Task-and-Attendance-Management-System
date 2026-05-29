# ASP.NET Core API 3-Layer Guide

This repository currently keeps database access and business rules inside controllers. That works for a small API, but it becomes hard to maintain as the project grows.

The next step is to split the API into 3 layers:

1. Controller: handles HTTP, authorization, route params, and returns responses.
2. Service: contains business logic and validation.
3. Repository: talks to Entity Framework Core and the database.

The goal is simple: controllers stay thin, services stay focused on rules, and repositories stay focused on data access.

## Recommended Folder Structure

You can keep the project clean with folders like this:

```text
Controllers/
Services/
  Interfaces/
Repositories/
  Interfaces/
DTOs/
Models/
```

For a task feature, the files could look like this:

```text
Services/Interfaces/ITaskService.cs
Services/TaskService.cs
Repositories/Interfaces/ITaskRepository.cs
Repositories/TaskRepository.cs
Controllers/TaskController.cs
```

## Step By Step

### 1. Move database logic out of the controller

Your current `TaskController` directly uses `EmployeeTaskAttendanceDbContext`. That means the controller is doing too much.

Move all EF Core calls like `context.Tasks.FirstOrDefault(...)`, `context.Tasks.Add(...)`, and `context.SaveChanges()` into a repository class.

### 2. Create a repository interface

The repository should only manage task data operations.

```csharp
public interface ITaskRepository
{
    List<Models.Task> GetAll();
    Models.Task? GetById(long id);
    List<Models.Task> GetByAssignee(int employeeId);
    void Add(Models.Task task);
    void Update(Models.Task task);
    void Delete(Models.Task task);
    bool UserExists(int userId);
    void Save();
}
```

### 3. Implement the repository

```csharp
using Employee_Task_and_Attendance_Management_System.Models;

namespace Employee_Task_and_Attendance_Management_System.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly EmployeeTaskAttendanceDbContext context;

    public TaskRepository(EmployeeTaskAttendanceDbContext context)
    {
        this.context = context;
    }

    public List<Models.Task> GetAll() => context.Tasks.ToList();

    public Models.Task? GetById(long id) => context.Tasks.FirstOrDefault(item => item.Id == id);

    public List<Models.Task> GetByAssignee(int employeeId) =>
        context.Tasks.Where(item => item.AssignedTo == employeeId).ToList();

    public void Add(Models.Task task) => context.Tasks.Add(task);

    public void Update(Models.Task task) => context.Tasks.Update(task);

    public void Delete(Models.Task task) => context.Tasks.Remove(task);

    public bool UserExists(int userId) => context.Users.Any(user => user.Id == userId);

    public void Save() => context.SaveChanges();
}
```

### 4. Create a service interface

The service should own the rules for creating, assigning, updating, and deleting tasks.

```csharp
using Employee_Task_and_Attendance_Management_System.DTOs.Tasks;

public interface ITaskService
{
    List<ResponseTaskDto> GetAll();
    List<ResponseTaskDto> GetMyTasks(int employeeId);
    ResponseTaskDto? GetById(long id);
    ResponseTaskDto Create(CreateTaskDto dto, int createdBy);
}
```

### 5. Implement the service

This example shows the create flow, because it is the easiest place to see the 3 layers working together.

```csharp
using Employee_Task_and_Attendance_Management_System.DTOs.Tasks;
using Employee_Task_and_Attendance_Management_System.Models;

namespace Employee_Task_and_Attendance_Management_System.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository repository;

    public TaskService(ITaskRepository repository)
    {
        this.repository = repository;
    }

    public List<ResponseTaskDto> GetAll()
    {
        return repository.GetAll()
            .Select(ToResponseTaskDto)
            .ToList();
    }

    public List<ResponseTaskDto> GetMyTasks(int employeeId)
    {
        return repository.GetByAssignee(employeeId)
            .Select(ToResponseTaskDto)
            .ToList();
    }

    public ResponseTaskDto? GetById(long id)
    {
        var task = repository.GetById(id);
        return task == null ? null : ToResponseTaskDto(task);
    }

    public ResponseTaskDto Create(CreateTaskDto dto, int createdBy)
    {
        var task = new Models.Task
        {
            Title = dto.Title,
            Description = dto.Description,
            AssignedTo = dto.AssignedTo,
            AssignedBy = createdBy,
            Priority = dto.Priority,
            Status = "Pending",
            Deadline = dto.Deadline,
            CreatedAt = DateTime.UtcNow
        };

        repository.Add(task);
        repository.Save();

        return ToResponseTaskDto(task);
    }

    private static ResponseTaskDto ToResponseTaskDto(Models.Task task)
    {
        return new ResponseTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            AssignedTo = task.AssignedTo,
            AssignedBy = task.AssignedBy,
            Priority = task.Priority,
            Status = task.Status,
            Deadline = task.Deadline,
            CreatedAt = task.CreatedAt
        };
    }
}
```

### 6. Make the controller thin

The controller should only handle HTTP concerns and call the service.

```csharp
using Employee_Task_and_Attendance_Management_System.DTOs.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Employee_Task_and_Attendance_Management_System.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly ITaskService service;

    public TaskController(ITaskService service)
    {
        this.service = service;
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpGet]
    public IActionResult GetTasks()
    {
        return Ok(service.GetAll());
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public IActionResult CreateTask(CreateTaskDto dto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(id, out var createdBy))
        {
            return Unauthorized();
        }

        var result = service.Create(dto, createdBy);
        return Ok(result);
    }
}
```

## 7. Register dependency injection

Add the interfaces and implementations in `Program.cs`.

```csharp
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
```

## 8. Test the flow

Once wired up, the request path becomes:

1. Client sends `POST /api/task`.
2. Controller reads the authenticated user id and calls the service.
3. Service creates the task and applies business rules.
4. Repository saves the task to the database.
5. Controller returns the response DTO.

## How Your Current Task Feature Maps

Using your existing code in `TaskController`, here is the best split:

1. Controller: JWT user id extraction, role checks, route handling, HTTP status codes.
2. Service: task creation, assignment rules, status updates, validation like user existence.
3. Repository: querying `Tasks` and `Users`, saving changes, database access only.

## Suggested Next Refactor Order

1. Start with `TaskController` because it already has clear CRUD logic.
2. Move read operations first: `GetTasks`, `GetTaskById`, `GetMyTask`.
3. Move write operations next: `CreateTask`, `UpdateTask`, `DeleteTask`.
4. Then move special actions: `AssignTask`, `UpdateTaskStatus`, `UpdateMyTaskStatus`.
5. Repeat the same pattern for `Attendance`, `Leaves`, `Department`, and `User`.

## Rule Of Thumb

If code reads or writes the database, put it in the repository.

If code decides what should happen, put it in the service.

If code receives an HTTP request or returns an HTTP response, put it in the controller.
