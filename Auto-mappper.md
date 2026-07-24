# Clean Architecture in ASP.NET Core: Step-by-Step Guide

This guide will help you implement best practices in your ASP.NET Core application by introducing a standardized API response model, global exception handling, FluentValidation, AutoMapper, and the thin controller pattern.

---

## 1. Standardized API Response Model

Returning a consistent JSON structure for every API endpoint makes it much easier for frontend clients to parse responses and handle errors gracefully.

### Step 1: Create the Response Model
Create a generic wrapper class that all your API responses will use.

```csharp
// Models/ApiResponse.cs
namespace StudentProjectManagementSystem.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        // Success Response Factory
        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T> { Success = true, Data = data, Message = message };
        }

        // Error Response Factory
        public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        {
            return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
        }
    }
}
```

---

## 2. Global Exception Middleware

Instead of writing `try-catch` blocks in every controller or service, use a Global Exception Middleware to catch unhandled exceptions globally and format them into our `ApiResponse` model.

### Step 1: Create the Middleware

```csharp
// Middlewares/ExceptionHandlingMiddleware.cs
using System.Net;
using System.Text.Json;
using StudentProjectManagementSystem.Models;

namespace StudentProjectManagementSystem.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Pass request to the next middleware
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            // Default to 500 Internal Server Error
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = ApiResponse<object>.Fail("An unexpected error occurred. Please try again later.");

            // You can customize status codes based on custom exception types
            if (exception is UnauthorizedAccessException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = ApiResponse<object>.Fail("Unauthorized access.");
            }
            else if (exception is KeyNotFoundException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = ApiResponse<object>.Fail("The requested resource was not found.");
            }

            var result = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(result);
        }
    }
}
```

### Step 2: Register Middleware in `Program.cs`
Add this line high up in your pipeline (before routing and controllers).

```csharp
// Program.cs
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## 3. FluentValidation for Request Validation

FluentValidation separates validation logic from your models and controllers. 

### Step 1: Install NuGet Packages
```bash
dotnet add package FluentValidation.AspNetCore
```

### Step 2: Create a DTO and Validator
Let's assume we are creating a `Project`.

```csharp
// DTOs/CreateProjectDto.cs
namespace StudentProjectManagementSystem.DTOs
{
    public class CreateProjectDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxStudents { get; set; }
    }
}
```

```csharp
// Validators/CreateProjectDtoValidator.cs
using FluentValidation;
using StudentProjectManagementSystem.DTOs;

namespace StudentProjectManagementSystem.Validators
{
    public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
    {
        public CreateProjectDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");

            RuleFor(x => x.MaxStudents)
                .GreaterThan(0).WithMessage("Max students must be at least 1.")
                .LessThanOrEqualTo(10).WithMessage("Max students cannot exceed 10.");
        }
    }
}
```

### Step 3: Register and Configure FluentValidation in `Program.cs`
We can also create a custom behavior to automatically format validation errors into our `ApiResponse` model if we are using MediatR, or configure ASP.NET Core's default model state validation.

For standard API Controllers:
```csharp
// Program.cs
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using StudentProjectManagementSystem.Models;

// Register FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // Registers all validators in the assembly

// Override default Model State behavior to use our ApiResponse
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .SelectMany(x => x.Value.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        var response = ApiResponse<object>.Fail("Validation Failed", errors);
        return new BadRequestObjectResult(response);
    };
});
```

---

## 4. AutoMapper for DTO ↔ Entity Conversion

AutoMapper eliminates the need to manually copy properties from DTOs to Entities and vice versa.

### Step 1: Install NuGet Package
```bash
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

### Step 2: Create a Mapping Profile
Create a profile that defines how classes map to each other.

```csharp
// Mappings/MappingProfile.cs
using AutoMapper;
using StudentProjectManagementSystem.DTOs;
using StudentProjectManagementSystem.Entities; // Assuming this is where your DB models are

namespace StudentProjectManagementSystem.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map from DTO to Entity (for Creation/Updates)
            CreateMap<CreateProjectDto, Project>();

            // Map from Entity to DTO (for Returning data to client)
            CreateMap<Project, ProjectResponseDto>();
        }
    }
}
```

### Step 3: Register AutoMapper in `Program.cs`
```csharp
// Program.cs
builder.Services.AddAutoMapper(typeof(Program));
```

---

## 5. Thin Controllers & Services

Controllers should **only**:
1. Receive the HTTP Request (Routing & Binding).
2. Call a Service to perform the actual business logic.
3. Return the `ApiResponse` wrapper.

### Step 1: The Service Interface and Implementation
The Service depends on the Database Context and AutoMapper.

```csharp
// Services/Interfaces/IProjectService.cs
using StudentProjectManagementSystem.DTOs;

namespace StudentProjectManagementSystem.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto);
        Task<ProjectResponseDto> GetProjectByIdAsync(int id);
    }
}
```

```csharp
// Services/ProjectService.cs
using AutoMapper;
using StudentProjectManagementSystem.DTOs;
using StudentProjectManagementSystem.Entities;
using StudentProjectManagementSystem.Data; // Assuming EF Core DbContext
using StudentProjectManagementSystem.Services.Interfaces;

namespace StudentProjectManagementSystem.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProjectService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto dto)
        {
            // 1. Map DTO to Entity using AutoMapper
            var projectEntity = _mapper.Map<Project>(dto);
            
            // 2. Perform DB operations
            _context.Projects.Add(projectEntity);
            await _context.SaveChangesAsync();

            // 3. Map Entity back to Response DTO
            return _mapper.Map<ProjectResponseDto>(projectEntity);
        }

        public async Task<ProjectResponseDto> GetProjectByIdAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            
            if (project == null)
            {
                // Our Global Exception Handler will catch this and return a 404
                throw new KeyNotFoundException($"Project with ID {id} not found."); 
            }

            return _mapper.Map<ProjectResponseDto>(project);
        }
    }
}
```

### Step 2: The Thin Controller
Notice how clean the controller is. It has no database logic, no explicit validation checks (handled by FluentValidation), and no explicit error handling (handled by Global Exception Middleware).

```csharp
// Controllers/ProjectsController.cs
using Microsoft.AspNetCore.Mvc;
using StudentProjectManagementSystem.DTOs;
using StudentProjectManagementSystem.Models;
using StudentProjectManagementSystem.Services.Interfaces;

namespace StudentProjectManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProjectResponseDto>>> CreateProject([FromBody] CreateProjectDto dto)
        {
            // Note: We don't need to check ModelState.IsValid here because 
            // FluentValidation AutoValidation handles it and returns our ApiResponse format!
            
            var result = await _projectService.CreateProjectAsync(dto);
            
            return CreatedAtAction(nameof(GetProject), new { id = result.Id }, ApiResponse<ProjectResponseDto>.Ok(result, "Project created successfully."));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProjectResponseDto>>> GetProject(int id)
        {
            var result = await _projectService.GetProjectByIdAsync(id);
            return Ok(ApiResponse<ProjectResponseDto>.Ok(result));
        }
    }
}
```

## Summary of the Flow

1. **Client sends Request** (e.g., `POST /api/projects`).
2. **FluentValidation** intercepts the request. 
   - If invalid, it short-circuits and returns a `400 BadRequest` in our `ApiResponse` format.
   - If valid, it proceeds to the Controller.
3. **Controller** receives the DTO and passes it directly to the **Service**.
4. **Service** uses **AutoMapper** to convert the DTO to a Database Entity.
5. **Service** performs business logic/database operations.
6. **Service** maps the Database Entity back to a read-only Response DTO and returns it.
7. **Controller** wraps the Response DTO in `ApiResponse.Ok()` and returns a `200` or `201`.
8. *If any error occurs anywhere*, the **Global Exception Middleware** catches it, logs it, and returns a sanitized `500` (or `404`/`401` based on exception type) in our `ApiResponse` format.
