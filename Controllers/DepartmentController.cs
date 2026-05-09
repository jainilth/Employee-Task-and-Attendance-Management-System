using Employee_Task_and_Attendance_Management_System.DTOs.Department;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Task_and_Attendance_Management_System.Controllers
{
    [Route("api/department")]
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
            var departments = context.Departments.Select(d => new
            {
                d.Id,
                d.Name
            }).ToList();
            return Ok(departments);
        }
        #endregion

        #region GetDepartmentById
        [HttpGet("{id}")]
        public IActionResult GetDepartmentById(int id)
        {
            var department = context.Departments.Select(d => new
            {
                d.Id,
                d.Name
            }).FirstOrDefault(d => d.Id == id);

            if (department == null)
            {
                return NotFound();
            }
            return Ok(department);
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
            var response = new ResponseDepartmentDto
            {
                Id = department.Id,
                Name = department.Name
            };
            return Ok(response);
        }
        #endregion

        #region UpdateDepartment
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateDepartment(int id, UpdateDepartmentDto updateDepartmentDto)
        {
            var department = context.Departments.FirstOrDefault(d => d.Id == id);
            if (department == null)
            {
                return NotFound();
            }
            department.Name = updateDepartmentDto.Name;
            context.SaveChanges();
            var response = new ResponseDepartmentDto
            {
                Id = department.Id,
                Name = department.Name
            };
            return Ok(response);
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
    }
}
