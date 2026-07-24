using Employee_Task_and_Attendance_Management_System.DTOs.Common;
using Employee_Task_and_Attendance_Management_System.DTOs.Department;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Employee_Task_and_Attendance_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly EmployeeTaskAttendanceDbContext context;
        public DepartmentController(EmployeeTaskAttendanceDbContext _context)
        {
            context = _context;
        }

        #region GetAllDepartments
        [HttpGet]
        public IActionResult GetDepartments([FromQuery] DepartmentQueryParameters parameters)
        {
            var query = context.Departments.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                query = query.Where(d => d.Name.Contains(parameters.SearchTerm));
            }

            var totalRecords = query.Count();

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "name" => parameters.IsDescending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                    _ => parameters.IsDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
                };
            }
            else
            {
                query = query.OrderBy(x => x.Id);
            }

            var departments = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(department => new ResponseDepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name
                })
                .ToList();

            var response = new PagedResponse<ResponseDepartmentDto>(departments, totalRecords, parameters.PageNumber, parameters.PageSize);

            return Ok(response);
        }
        #endregion

        #region GetDepartmentById
        [HttpGet("{id}")]
        public IActionResult GetDepartmentById(int id)
        {
            var department = context.Departments.FirstOrDefault(d => d.Id == id);

            if (department == null)
            {
                return NotFound();
            }

            return Ok(ToResponseDepartmentDto(department));
        }
        #endregion

        #region CreateDepartment
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult CreateDepartment(CreateDepartmentDto createDepartmentDto)
        {
            var department = new Department
            {
                Name = createDepartmentDto.Name
            };
            context.Departments.Add(department);
            context.SaveChanges();
            return Ok(ToResponseDepartmentDto(department));
        }
        #endregion

        #region UpdateDepartment
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}")]
        public IActionResult UpdateDepartment(int id, UpdateDepartmentDto updateDepartmentDto)
        {
            var department = context.Departments.FirstOrDefault(d => d.Id == id);
            if (department == null)
            {
                return NotFound();
            }
            department.Name = updateDepartmentDto.Name;
            context.SaveChanges();
            return Ok(ToResponseDepartmentDto(department));
        }
        #endregion

        #region DeleteDepartment
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteDepartment(int id)
        {
            var department = context.Departments.FirstOrDefault(d => d.Id == id);
            if (department == null)
            {
                return NotFound();
            }
            context.Departments.Remove(department);
            context.SaveChanges();
            return NoContent();

        }
        #endregion

        #region ResponseMapping
        private static ResponseDepartmentDto ToResponseDepartmentDto(Department department)
        {
            return new ResponseDepartmentDto
            {
                Id = department.Id,
                Name = department.Name
            };
        }
        #endregion
    }
}

