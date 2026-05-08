using Employee_Task_and_Attendance_Management_System.DTOs.Tasks;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Task_and_Attendance_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly EmployeeTaskAttendanceDbContext context;

        public TaskController(EmployeeTaskAttendanceDbContext _context)
        {
            context = _context;
        }

        #region GetAllTasks
        [HttpGet]
        public IActionResult GetTasks()
        {
            var tasks = context.Tasks.Select(task => new ResponseTaskDto
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
            }).ToList();

            return Ok(tasks);
        }
        #endregion

        #region GetTaskById
        [HttpGet("{id}")]
        public IActionResult GetTaskById(long id)
        {
            var task = context.Tasks.Select(item => new ResponseTaskDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                AssignedTo = item.AssignedTo,
                AssignedBy = item.AssignedBy,
                Priority = item.Priority,
                Status = item.Status,
                Deadline = item.Deadline,
                CreatedAt = item.CreatedAt
            }).FirstOrDefault(item => item.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return Ok(task);
        }
        #endregion

        #region CreateTask
        [HttpPost]
        public IActionResult CreateTask(CreateTaskDto createTaskDto)
        {
            if (!context.Users.Any(user => user.Id == createTaskDto.AssignedTo) || !context.Users.Any(user => user.Id == createTaskDto.AssignedBy))
            {
                return NotFound("Assigned user not found.");
            }

            var task = new Models.Task
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                AssignedTo = createTaskDto.AssignedTo,
                AssignedBy = createTaskDto.AssignedBy,
                Priority = createTaskDto.Priority,
                Status = createTaskDto.Status,
                Deadline = createTaskDto.Deadline,
                CreatedAt = DateTime.UtcNow
            };

            context.Tasks.Add(task);
            context.SaveChanges();

            var response = new ResponseTaskDto
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

            return Ok(response);
        }
        #endregion

        #region UpdateTask
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
                return NotFound("Assigned user not found.");
            }

            task.Title = updateTaskDto.Title;
            task.Description = updateTaskDto.Description;
            task.AssignedTo = updateTaskDto.AssignedTo;
            task.AssignedBy = updateTaskDto.AssignedBy;
            task.Priority = updateTaskDto.Priority;
            task.Status = updateTaskDto.Status;
            task.Deadline = updateTaskDto.Deadline;

            context.SaveChanges();

            var response = new ResponseTaskDto
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

            return Ok(response);
        }
        #endregion

        #region DeleteTask
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
    }
}
