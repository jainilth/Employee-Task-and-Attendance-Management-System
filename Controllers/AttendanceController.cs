using Employee_Task_and_Attendance_Management_System.DTOs.Attendance;
using Employee_Task_and_Attendance_Management_System.DTOs.Common;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult GetAttendances([FromQuery] AttendanceQueryParameters parameters)
        {
            var query = context.Attendances.AsNoTracking().AsQueryable();

            if (parameters.EmployeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeId == parameters.EmployeeId.Value);
            }

            if (parameters.Date.HasValue)
            {
                query = query.Where(a => a.Date == parameters.Date.Value);
            }

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(a => a.Status == parameters.Status);
            }

            var totalRecords = query.Count();

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "date" => parameters.IsDescending ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
                    "checkin" => parameters.IsDescending ? query.OrderByDescending(x => x.CheckIn) : query.OrderBy(x => x.CheckIn),
                    "checkout" => parameters.IsDescending ? query.OrderByDescending(x => x.CheckOut) : query.OrderBy(x => x.CheckOut),
                    "status" => parameters.IsDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    _ => parameters.IsDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
                };
            }
            else
            {
                query = query.OrderBy(x => x.Id);
            }

            var attendances = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(attendance => new ResponseAttendanceDto
                {
                    Id = attendance.Id,
                    EmployeeId = attendance.EmployeeId,
                    CheckIn = attendance.CheckIn,
                    CheckOut = attendance.CheckOut,
                    WorkingHours = attendance.WorkingHours,
                    Status = attendance.Status,
                    Date = attendance.Date
                })
                .ToList();

            var response = new PagedResponse<ResponseAttendanceDto>(attendances, totalRecords, parameters.PageNumber, parameters.PageSize);

            return Ok(response);
        }
        #endregion

        #region GetallAttendenceSelf
        [HttpGet("self")]
        public IActionResult GetMyAttendance([FromQuery] AttendanceQueryParameters parameters)
        {
            var IdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(IdClaim, out var id))
            {
                return Unauthorized();
            }

            var query = context.Attendances.AsNoTracking().Where(at => at.EmployeeId == id).AsQueryable();

            if (parameters.Date.HasValue)
            {
                query = query.Where(a => a.Date == parameters.Date.Value);
            }

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(a => a.Status == parameters.Status);
            }

            var totalRecords = query.Count();

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "date" => parameters.IsDescending ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date),
                    "checkin" => parameters.IsDescending ? query.OrderByDescending(x => x.CheckIn) : query.OrderBy(x => x.CheckIn),
                    "checkout" => parameters.IsDescending ? query.OrderByDescending(x => x.CheckOut) : query.OrderBy(x => x.CheckOut),
                    "status" => parameters.IsDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    _ => parameters.IsDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
                };
            }
            else
            {
                query = query.OrderBy(x => x.Id);
            }

            var attendances = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(attendance => new ResponseAttendanceDto
                {
                    Id = attendance.Id,
                    EmployeeId = attendance.EmployeeId,
                    CheckIn = attendance.CheckIn,
                    CheckOut = attendance.CheckOut,
                    WorkingHours = attendance.WorkingHours,
                    Status = attendance.Status,
                    Date = attendance.Date
                })
                .ToList();

            var response = new PagedResponse<ResponseAttendanceDto>(attendances, totalRecords, parameters.PageNumber, parameters.PageSize);

            return Ok(response);
        }
        #endregion

        #region GetAttendancePerEmployee
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("employee/{id:int}")]
        public IActionResult GetAttendanceByEmployeeId(int id)
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
        [HttpGet("employee/{id:int}/{date}")]
        public IActionResult GetAttendanceByEmployeeIdAndDate(int id, DateOnly date)
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