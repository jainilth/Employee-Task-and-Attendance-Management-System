using Microsoft.AspNetCore.Mvc;

namespace Employee_Task_and_Attendance_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        [HttpGet]
        public IActionResult GetEmployees()
        {
            var employees = new[]
            {
                new
                {
                    Id = 1,
                    Name = "Jainil",
                    Department = "IT"
                }
            };

            return Ok(employees);
        }

        [HttpPost]
        public IActionResult CreateEmployee()
        {
            return Ok("Employee Created");
        }
    }
}
