using Employee_Task_and_Attendance_Management_System.DTOs.Tasks;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
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
        public IActionResult GetTasks()
        {
            var tasks = context.Tasks
                .ToList()
                .Select(ToResponseTaskDto)
                .ToList();

            return Ok(tasks);
        }
        #endregion

        #region GetMyTask
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("my")]
        public IActionResult GetMyTask()
        {
            var eid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(eid, out var empid))
            {
                return Unauthorized();
            }

            var tasks = context.Tasks
                .Where(item => item.AssignedTo == empid)
                .ToList()
                .Select(ToResponseTaskDto)
                .ToList();

            if (!tasks.Any())
            {
                return NotFound();
            }

            return Ok(tasks);
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

            if (!context.Users.Any(user => user.Id == updateTaskDto.AssignedTo) || !context.Users.Any(user => user.Id == updateTaskDto.AssignedBy))
            {
                return NotFound("user not found.");
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
