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
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public IActionResult GetAttendances()
        {
            var attendances = context.Attendances
                .ToList()
                .Select(ToResponseAttendanceDto)
                .ToList();

            return Ok(attendances);
        }
        #endregion

        #region GetallAttendenceSelf
        [HttpGet("self")]
        public IActionResult GetMyAttendance()
        {
            var Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(Id, out var id))
            {
                return Unauthorized();
            }
            var Attendence = context.Attendances
                .Where(at => at.EmployeeId == id)
                .ToList()
                .Select(ToResponseAttendanceDto)
                .ToList();

            return Ok(Attendence);
        }
        #endregion

        #region GetAttendancePerEmployee
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("employee/{id:long}")]
        public IActionResult GetAttendanceByEmployeeId(long id)
        {
            var attendance = context.Attendances
                .Where(item => item.EmployeeId == id)
                .ToList()
                .Select(ToResponseAttendanceDto)
                .ToList();

            if (!attendance.Any())
            {
                return NotFound("No attendance found for this employee.");
            }

            return Ok(attendance);
        }
        #endregion

        #region GetAttendancePerEmployeeAndDate
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("employee/{id:long}/{date}")]
        public IActionResult GetAttendanceByEmployeeIdAndDate(long id, DateOnly date)
        {
            var attendance = context.Attendances
                .Where(item => item.EmployeeId == id && item.Date == date)
                .ToList()
                .Select(ToResponseAttendanceDto)
                .ToList();

            if (!attendance.Any())
            {
                return NotFound("No attendance found for this employee on this date.");
            }

            return Ok(attendance);
        }
        #endregion

        #region GetAttendancePerStatus
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("status/{status}")]
        public IActionResult GetAttendanceByStatus(string status)
        {
            var attendance = context.Attendances
                .Where(item => item.Status == status)
                .ToList()
                .Select(ToResponseAttendanceDto)
                .ToList();

            if (!attendance.Any())
            {
                return NotFound("No attendance found with this status.");
            }

            return Ok(attendance);
        }
        #endregion

        #region CheckIn
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
        [HttpPatch("checkout")]
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

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (context.Attendances.Any(attendance => attendance.EmployeeId == createAttendanceDto.EmployeeId && attendance.Date == today))
            {
                return Conflict("Attendance already exists for today.");
            }

            var attendance = new Attendance
            {
                EmployeeId = createAttendanceDto.EmployeeId,
                CheckIn = createAttendanceDto.CheckIn,
                CheckOut = createAttendanceDto.CheckOut,
                WorkingHours = createAttendanceDto.CheckOut.HasValue
                    ? Math.Round((decimal)(createAttendanceDto.CheckOut.Value - createAttendanceDto.CheckIn).TotalHours, 2)
                    : null,
                Status = createAttendanceDto.Status,
                Date = createAttendanceDto.Date
            };


            context.Attendances.Add(attendance);
            context.SaveChanges();

            return Ok(ToResponseAttendanceDto(attendance));
        }
        #endregion

        #region UpdateAttendanceStatusOfEmployee
        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("{id:long}/employee/{date}/{status}")]
        public IActionResult UpdateAttendance(long id, DateOnly date, string status)
        {
            if (!context.Users.Any(u => u.Id == id))
            {
                return NotFound("Employee not found.");
            }

            var attendance = context.Attendances
                .FirstOrDefault(a => a.EmployeeId == id && a.Date == date);

            if (attendance == null)
            {
                return NotFound("Attendance record not found.");
            }

            var validStatuses = new[]
            {
                "Present",
                "Absent",
                "Late",
                "HalfDay",
                "OnLeave"
            };

            status = status.Trim();

            if (!validStatuses.Contains(status))
            {
                return BadRequest("Invalid status.");
            }

            attendance.Status = status;

            context.SaveChanges();

            return Ok(ToResponseAttendanceDto(attendance));
        }
        #endregion

        #region DeleteAttendance
        [Authorize(Roles = "Admin,Manager")]
        [HttpDelete("{id:long}")]
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

        #region ResponseMapping
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
        #endregion
    }
}