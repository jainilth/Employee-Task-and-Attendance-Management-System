using Employee_Task_and_Attendance_Management_System.DTOs.Attendance;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Task_and_Attendance_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly EmployeeTaskAttendanceDbContext context;

        public AttendanceController(EmployeeTaskAttendanceDbContext _context)
        {
            context = _context;
        }

        #region GetAllAttendances
        [HttpGet]
        public IActionResult GetAttendances()
        {
            var attendances = context.Attendances.Select(attendance => new ResponseAttendanceDto
            {
                Id = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                WorkingHours = attendance.WorkingHours,
                Status = attendance.Status,
                Date = attendance.Date
            }).ToList();

            return Ok(attendances);
        }
        #endregion

        #region GetAttendanceById
        [HttpGet("{id}")]
        public IActionResult GetAttendanceById(long id)
        {
            var attendance = context.Attendances.Select(item => new ResponseAttendanceDto
            {
                Id = item.Id,
                EmployeeId = item.EmployeeId,
                CheckIn = item.CheckIn,
                CheckOut = item.CheckOut,
                WorkingHours = item.WorkingHours,
                Status = item.Status,
                Date = item.Date
            }).FirstOrDefault(item => item.Id == id);

            if (attendance == null)
            {
                return NotFound();
            }

            return Ok(attendance);
        }
        #endregion

        #region CreateAttendance
        [HttpPost]
        public IActionResult CreateAttendance(CreateAttendanceDto createAttendanceDto)
        {
            if (!context.Users.Any(user => user.Id == createAttendanceDto.EmployeeId))
            {
                return NotFound("Employee not found.");
            }

            var attendance = new Attendance
            {
                EmployeeId = createAttendanceDto.EmployeeId,
                CheckIn = createAttendanceDto.CheckIn,
                CheckOut = createAttendanceDto.CheckOut,
                WorkingHours = createAttendanceDto.WorkingHours,
                Status = createAttendanceDto.Status,
                Date = createAttendanceDto.Date
            };

            context.Attendances.Add(attendance);
            context.SaveChanges();

            var response = new ResponseAttendanceDto
            {
                Id = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                WorkingHours = attendance.WorkingHours,
                Status = attendance.Status,
                Date = attendance.Date
            };

            return Ok(response);
        }
        #endregion

        #region UpdateAttendance
        [HttpPut("{id}")]
        public IActionResult UpdateAttendance(long id, UpdateAttendanceDto updateAttendanceDto)
        {
            var attendance = context.Attendances.FirstOrDefault(item => item.Id == id);
            if (attendance == null)
            {
                return NotFound();
            }

            if (!context.Users.Any(user => user.Id == updateAttendanceDto.EmployeeId))
            {
                return NotFound("Employee not found.");
            }

            attendance.EmployeeId = updateAttendanceDto.EmployeeId;
            attendance.CheckIn = updateAttendanceDto.CheckIn;
            attendance.CheckOut = updateAttendanceDto.CheckOut;
            attendance.WorkingHours = updateAttendanceDto.WorkingHours;
            attendance.Status = updateAttendanceDto.Status;
            attendance.Date = updateAttendanceDto.Date;

            context.SaveChanges();

            var response = new ResponseAttendanceDto
            {
                Id = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                WorkingHours = attendance.WorkingHours,
                Status = attendance.Status,
                Date = attendance.Date
            };

            return Ok(response);
        }
        #endregion

        #region DeleteAttendance
        [HttpDelete("{id}")]
        public IActionResult DeleteAttendance(long id)
        {
            var attendance = context.Attendances.FirstOrDefault(item => item.Id == id);
            if (attendance == null)
            {
                return NotFound();
            }

            context.Attendances.Remove(attendance);
            context.SaveChanges();

            return NoContent();
        }
        #endregion
    }
}