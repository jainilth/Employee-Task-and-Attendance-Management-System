using Employee_Task_and_Attendance_Management_System.DTOs.Department;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult GetDepartments()
        {
            var departments = context.Departments
                .ToList()
                .Select(ToResponseDepartmentDto)
                .ToList();

            return Ok(departments);
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

