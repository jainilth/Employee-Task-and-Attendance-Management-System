using BCrypt.Net;
using Employee_Task_and_Attendance_Management_System.DTOs.User;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Task_and_Attendance_Management_System.Controllers
{
    [ApiController]
    [Route("api/employee")]
    [Authorize]
    public class UserController : ControllerBase
    {

        private readonly EmployeeTaskAttendanceDbContext context;
        public UserController(EmployeeTaskAttendanceDbContext _context)
        {
            context = _context;
        }

        #region GetAllUsers
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = context.Users.Select(user => new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            }).ToList();

            return Ok(users);
        }
        #endregion

        #region GetUserById
        [Authorize(Roles = "Admin,Manager,Employee")]
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = context.Users.Select(item => new UserResponseDto
            {
                Id = item.Id,
                Name = item.Name,
                Email = item.Email,
                Role = item.Role,
                DepartmentId = item.DepartmentId
            }).FirstOrDefault(item => item.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        #endregion

        #region CreateUser
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult CreateUser(UserCreateDto userCreateDto)
        {
            if (userCreateDto.DepartmentId.HasValue && !context.Departments.Any(department => department.Id == userCreateDto.DepartmentId.Value))
            {
                return NotFound("Department not found.");
            }

            var user = new User
            {
                Name = userCreateDto.Name,
                Email = userCreateDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDto.PasswordHash),
                Role = userCreateDto.Role,
                DepartmentId = userCreateDto.DepartmentId
            };

            context.Users.Add(user);
            context.SaveChanges();

            var response = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            };

            return Ok(response);
        }
        #endregion

        #region UpdateUser
        [Authorize(Roles = "Admin,Employee")]
        [HttpPatch("{id}")]
        public IActionResult UpdateUser(int id, UserUpdateDto userUpdateDto)
        {
            var user = context.Users.FirstOrDefault(item => item.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (userUpdateDto.DepartmentId.HasValue && !context.Departments.Any(department => department.Id == userUpdateDto.DepartmentId.Value))
            {
                return NotFound("Department not found.");
            }

            user.Name = userUpdateDto.Name;
            user.Email = userUpdateDto.Email;
            user.Role = userUpdateDto.Role;
            user.DepartmentId = userUpdateDto.DepartmentId;
            context.SaveChanges();

            var response = new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            };

            return Ok(response);
        }
        #endregion

        #region DeleteUser
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = context.Users.FirstOrDefault(item => item.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            context.Users.Remove(user);
            context.SaveChanges();

            return NoContent();
        }
        #endregion
    }
}
