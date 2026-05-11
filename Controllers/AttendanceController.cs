using Employee_Task_and_Attendance_Management_System.DTOs.Attendance;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Employee_Task_and_Attendance_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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

        #region CheckIn
        [Authorize]
        [HttpPost("checkin")]
        public IActionResult CheckIn()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var employeeId))
            {
                return Unauthorized();
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (context.Attendances.Any(attendance => attendance.EmployeeId == employeeId && attendance.Date == today))
            {
                return Conflict("Attendance already exists for today.");
            }

            var attendanceRecord = new Attendance
            {
                EmployeeId = employeeId,
                CheckIn = DateTime.UtcNow,
                CheckOut = null,
                WorkingHours = null,
                Status = "Present",
                Date = today
            };

            context.Attendances.Add(attendanceRecord);
            context.SaveChanges();

            return Ok(ToResponseAttendanceDto(attendanceRecord));
        }
        #endregion

        #region CheckOut
        [Authorize]
        [HttpPost("checkout")]
        public IActionResult CheckOut()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var employeeId))
            {
                return Unauthorized();
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var attendanceRecord = context.Attendances.FirstOrDefault(attendance => attendance.EmployeeId == employeeId && attendance.Date == today);

            if (attendanceRecord == null)
            {
                return NotFound("Attendance record not found for today.");
            }

            if (attendanceRecord.CheckOut.HasValue)
            {
                return Conflict("Attendance already checked out.");
            }

            var checkOutTime = DateTime.UtcNow;

            if (checkOutTime < attendanceRecord.CheckIn)
            {
                return BadRequest("Check-out time cannot be earlier than check-in time.");
            }

            attendanceRecord.CheckOut = checkOutTime;
            attendanceRecord.WorkingHours = Math.Round((decimal)(attendanceRecord.CheckOut.Value - attendanceRecord.CheckIn).TotalHours, 2);

            context.SaveChanges();

            return Ok(ToResponseAttendanceDto(attendanceRecord));
        }
        #endregion

        #region CreateAttendance
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin,Manager")]
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
        [Authorize(Roles = "Admin")]
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

        private static ResponseAttendanceDto ToResponseAttendanceDto(Attendance attendance)
        {
            return new ResponseAttendanceDto
            {
                Id = attendance.Id,
                EmployeeId = attendance.EmployeeId,
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                WorkingHours = attendance.WorkingHours,
                Status = attendance.Status,
                Date = attendance.Date
            };
        }
    }
}