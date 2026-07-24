using BCrypt.Net;
using Employee_Task_and_Attendance_Management_System.DTOs.Common;
using Employee_Task_and_Attendance_Management_System.DTOs.User;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Employee_Task_and_Attendance_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public IActionResult GetUsers([FromQuery] UserQueryParameters parameters)
        {
            var query = context.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(parameters.Role))
            {
                query = query.Where(u => u.Role == parameters.Role);
            }

            if (parameters.DepartmentId.HasValue)
            {
                query = query.Where(u => u.DepartmentId == parameters.DepartmentId.Value);
            }

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                query = query.Where(u => u.Name.Contains(parameters.SearchTerm) || u.Email.Contains(parameters.SearchTerm));
            }

            var totalRecords = query.Count();

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "name" => parameters.IsDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                    "email" => parameters.IsDescending ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
                    "role" => parameters.IsDescending ? query.OrderByDescending(x => x.Role) : query.OrderBy(x => x.Role),
                    _ => parameters.IsDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
                };
            }
            else
            {
                query = query.OrderBy(x => x.Id);
            }

            var users = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(user => new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    DepartmentId = user.DepartmentId
                })
                .ToList();

            var response = new PagedResponse<UserResponseDto>(users, totalRecords, parameters.PageNumber, parameters.PageSize);

            return Ok(response);
        }
        #endregion

        #region GetUserById
        [Authorize(Roles = "Admin,Manager,Employee")]
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = context.Users.FirstOrDefault(item => item.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(ToUserResponseDto(user));
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

            return Ok(ToUserResponseDto(user));
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

            return Ok(ToUserResponseDto(user));
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

        #region AssignRole
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/role")]
        public IActionResult AssignRole(int id, string role)
        {
            var user = context.Users.FirstOrDefault(item => item.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            user.Role = role;
            context.SaveChanges();

            return Ok(ToUserResponseDto(user));
        }
        #endregion

        #region AssignDepartment
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/department")]
        public IActionResult AssignDepartment(int id, int departmentId)
        {
            var user = context.Users.FirstOrDefault(item => item.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            user.DepartmentId = departmentId;
            context.SaveChanges();

            return Ok(ToUserResponseDto(user));
        }
        #endregion

        #region ResponseMapping
        private static UserResponseDto ToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            };
        }
        #endregion
    }
}

