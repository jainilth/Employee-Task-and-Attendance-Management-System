using Employee_Task_and_Attendance_Management_System.DTOs.Common;
using Employee_Task_and_Attendance_Management_System.DTOs.Tasks;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Employee_Task_and_Attendance_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly EmployeeTaskAttendanceDbContext context;

        public TaskController(EmployeeTaskAttendanceDbContext _context)
        {
            context = _context;
        }

        #region GetAllTasks
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public IActionResult GetTasks([FromQuery] TaskQueryParameters parameters)
        {
            var query = context.Tasks.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(t => t.Status == parameters.Status);
            }

            if (!string.IsNullOrEmpty(parameters.Priority))
            {
                query = query.Where(t => t.Priority == parameters.Priority);
            }

            if (parameters.AssignedTo.HasValue)
            {
                query = query.Where(t => t.AssignedTo == parameters.AssignedTo.Value);
            }

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                query = query.Where(t => t.Title.Contains(parameters.SearchTerm) || (t.Description != null && t.Description.Contains(parameters.SearchTerm)));
            }

            var totalRecords = query.Count();

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "title" => parameters.IsDescending ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
                    "priority" => parameters.IsDescending ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority),
                    "status" => parameters.IsDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "deadline" => parameters.IsDescending ? query.OrderByDescending(x => x.Deadline) : query.OrderBy(x => x.Deadline),
                    "createdat" => parameters.IsDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                    _ => parameters.IsDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
                };
            }
            else
            {
                query = query.OrderBy(x => x.Id);
            }

            var tasks = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(task => new ResponseTaskDto
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
                })
                .ToList();

            var response = new PagedResponse<ResponseTaskDto>(tasks, totalRecords, parameters.PageNumber, parameters.PageSize);

            return Ok(response);
        }
        #endregion

        #region GetMyTask
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("my")]
        public IActionResult GetMyTask([FromQuery] TaskQueryParameters parameters)
        {
            var eid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(eid, out var empid))
            {
                return Unauthorized();
            }

            var query = context.Tasks.AsNoTracking().Where(item => item.AssignedTo == empid).AsQueryable();

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(t => t.Status == parameters.Status);
            }

            if (!string.IsNullOrEmpty(parameters.Priority))
            {
                query = query.Where(t => t.Priority == parameters.Priority);
            }

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                query = query.Where(t => t.Title.Contains(parameters.SearchTerm) || (t.Description != null && t.Description.Contains(parameters.SearchTerm)));
            }

            var totalRecords = query.Count();

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "title" => parameters.IsDescending ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
                    "priority" => parameters.IsDescending ? query.OrderByDescending(x => x.Priority) : query.OrderBy(x => x.Priority),
                    "status" => parameters.IsDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    "deadline" => parameters.IsDescending ? query.OrderByDescending(x => x.Deadline) : query.OrderBy(x => x.Deadline),
                    "createdat" => parameters.IsDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                    _ => parameters.IsDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
                };
            }
            else
            {
                query = query.OrderBy(x => x.Id);
            }

            var tasks = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(task => new ResponseTaskDto
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
                })
                .ToList();

            if (!tasks.Any())
            {
                return NotFound();
            }

            var response = new PagedResponse<ResponseTaskDto>(tasks, totalRecords, parameters.PageNumber, parameters.PageSize);

            return Ok(response);
        }
        #endregion

        #region GetTaskById
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{id}")]
        public IActionResult GetTaskById(long id)
        {
            var task = context.Tasks.FirstOrDefault(item => item.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(ToResponseTaskDto(task));
        }
        #endregion

        #region CreateTask
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult CreateTask(CreateTaskDto createTaskDto)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(id, out var eid))
            {
                return Unauthorized();
            }

            var task = new Models.Task
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                AssignedTo = createTaskDto.AssignedTo,
                AssignedBy = eid,
                Priority = createTaskDto.Priority,
                Status = "Pending",
                Deadline = createTaskDto.Deadline,
                CreatedAt = DateTime.UtcNow
            };

            context.Tasks.Add(task);
            context.SaveChanges();

            return Ok(ToResponseTaskDto(task));
        }
        #endregion

        #region UpdateTask
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateTask(long id, UpdateTaskDto updateTaskDto)
        {
            var task = context.Tasks.FirstOrDefault(item => item.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            if (updateTaskDto.AssignedTo.HasValue && !context.Users.Any(user => user.Id == updateTaskDto.AssignedTo.Value))
            {
                return NotFound("Assigned user not found.");
            }

            if (!context.Users.Any(user => user.Id == updateTaskDto.AssignedBy))
            {
                return NotFound("AssignedBy user not found.");
            }

            task.Title = updateTaskDto.Title;
            task.Description = updateTaskDto.Description;
            task.AssignedTo = updateTaskDto.AssignedTo;
            task.AssignedBy = updateTaskDto.AssignedBy;
            task.Priority = updateTaskDto.Priority;
            task.Status = updateTaskDto.Status;
            task.Deadline = updateTaskDto.Deadline;

            context.SaveChanges();

            return Ok(ToResponseTaskDto(task));
        }
        #endregion

        #region AssignTaks
        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("{id}/assign")]
        public IActionResult AssignTask(long id, long AssignTo)
        {
            var eid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(eid, out var AssignBy))
            {
                return Unauthorized();
            }

            var task = context.Tasks.FirstOrDefault(item => item.Id == id);
            if (task == null)
            {
                return NotFound();
            }
            if (!context.Users.Any(user => user.Id == AssignTo))
            {
                return NotFound("Assigned user not found.");
            }
            task.AssignedTo = (int)AssignTo;
            task.AssignedBy = AssignBy;
            context.SaveChanges();
            return Ok(ToResponseTaskDto(task));
        }
        #endregion

        #region UpdateTaskStatus
        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("{id:long}/{status}")]
        public IActionResult UpdateTaskStatus(long id, string status)
        {
            var task = context.Tasks.FirstOrDefault(item => item.Id == id);
            if (task == null)
            {
                return NotFound();
            }
            task.Status = status;
            context.SaveChanges();
            return Ok(ToResponseTaskDto(task));
        }
        #endregion

        #region UpdateMyTaskStatus
        [HttpPatch("my/{id:long}/{status}")]
        public IActionResult UpdateMyTaskStatus(long id, string status)
        {
            var eid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(eid, out var empid))
            {
                return Unauthorized();
            }

            var task = context.Tasks.FirstOrDefault(item => item.Id == id && item.AssignedTo == empid);
            if (task == null)
            {
                return NotFound();
            }
            task.Status = status;
            context.SaveChanges();
            return Ok(ToResponseTaskDto(task));
        }
        #endregion

        #region DeleteTask
        [Authorize(Roles = "Admin,Manager")]
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(long id)
        {
            var task = context.Tasks.FirstOrDefault(item => item.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            context.Tasks.Remove(task);
            context.SaveChanges();

            return NoContent();
        }
        #endregion

        #region ResponseMapping
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
        #endregion
    }
}
