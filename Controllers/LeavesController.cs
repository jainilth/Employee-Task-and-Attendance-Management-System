using Employee_Task_and_Attendance_Management_System.DTOs.Common;
using Employee_Task_and_Attendance_Management_System.DTOs.Leaves;
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
    public class LeavesController : ControllerBase
    {
        private readonly EmployeeTaskAttendanceDbContext context;

        public LeavesController(EmployeeTaskAttendanceDbContext _context)
        {
            context = _context;
        }

        #region GetAllLeaves
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public IActionResult GetLeaves([FromQuery] LeaveQueryParameters parameters)
        {
            var query = context.Leaves.AsNoTracking().AsQueryable();

            if (parameters.EmployeeId.HasValue)
            {
                query = query.Where(l => l.EmployeeId == parameters.EmployeeId.Value);
            }

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(l => l.Status == parameters.Status);
            }

            if (!string.IsNullOrEmpty(parameters.LeaveType))
            {
                query = query.Where(l => l.LeaveType == parameters.LeaveType);
            }

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                query = query.Where(l => l.Reason != null && l.Reason.Contains(parameters.SearchTerm));
            }

            var totalRecords = query.Count();

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "leavetype" => parameters.IsDescending ? query.OrderByDescending(x => x.LeaveType) : query.OrderBy(x => x.LeaveType),
                    "startdate" => parameters.IsDescending ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate),
                    "enddate" => parameters.IsDescending ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
                    "status" => parameters.IsDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    _ => parameters.IsDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
                };
            }
            else
            {
                query = query.OrderBy(x => x.Id);
            }

            var leaves = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(leave => new ResponseLeaveDto
                {
                    Id = leave.Id,
                    EmployeeId = leave.EmployeeId,
                    LeaveType = leave.LeaveType,
                    StartDate = leave.StartDate,
                    EndDate = leave.EndDate,
                    Reason = leave.Reason,
                    Status = leave.Status
                })
                .ToList();

            var response = new PagedResponse<ResponseLeaveDto>(leaves, totalRecords, parameters.PageNumber, parameters.PageSize);

            return Ok(response);
        }
        #endregion

        #region GetLeaveSelf
        [HttpGet("self")]
        public IActionResult GetMyLeaves([FromQuery] LeaveQueryParameters parameters)
        {
            var IdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(IdClaim, out var eid))
            {
                return Unauthorized();
            }

            var query = context.Leaves.AsNoTracking().Where(item => item.EmployeeId == eid).AsQueryable();

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(l => l.Status == parameters.Status);
            }

            if (!string.IsNullOrEmpty(parameters.LeaveType))
            {
                query = query.Where(l => l.LeaveType == parameters.LeaveType);
            }

            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                query = query.Where(l => l.Reason != null && l.Reason.Contains(parameters.SearchTerm));
            }

            var totalRecords = query.Count();

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "leavetype" => parameters.IsDescending ? query.OrderByDescending(x => x.LeaveType) : query.OrderBy(x => x.LeaveType),
                    "startdate" => parameters.IsDescending ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate),
                    "enddate" => parameters.IsDescending ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
                    "status" => parameters.IsDescending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
                    _ => parameters.IsDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
                };
            }
            else
            {
                query = query.OrderBy(x => x.Id);
            }

            var leave = query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(leave => new ResponseLeaveDto
                {
                    Id = leave.Id,
                    EmployeeId = leave.EmployeeId,
                    LeaveType = leave.LeaveType,
                    StartDate = leave.StartDate,
                    EndDate = leave.EndDate,
                    Reason = leave.Reason,
                    Status = leave.Status
                })
                .ToList();

            if (!leave.Any())
            {
                return NotFound();
            }

            var response = new PagedResponse<ResponseLeaveDto>(leave, totalRecords, parameters.PageNumber, parameters.PageSize);

            return Ok(response);
        }
        #endregion

        #region GetLeaveById
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("{id:long}")]
        public IActionResult GetLeaveById(long id)
        {
            var leave = context.Leaves.FirstOrDefault(item => item.Id == id);

            if (leave == null)
            {
                return NotFound();
            }

            return Ok(ToResponseLeafDto(leave));
        }
        #endregion

        #region CreateLeave
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult CreateLeave(CreateLeaveDto createLeafDto)
        {
            if (!context.Users.Any(user => user.Id == createLeafDto.EmployeeId))
            {
                return NotFound("Employee not found.");
            }

            var leave = new Leaf
            {
                EmployeeId = createLeafDto.EmployeeId,
                LeaveType = createLeafDto.LeaveType,
                StartDate = createLeafDto.StartDate,
                EndDate = createLeafDto.EndDate,
                Reason = createLeafDto.Reason,
                Status = createLeafDto.Status
            };

            context.Leaves.Add(leave);
            context.SaveChanges();

            return Ok(ToResponseLeafDto(leave));
        }
        #endregion

        #region ApplyForLeave
        [HttpPost("apply")]
        public IActionResult ApplyForLeave(ApplyLeaveDto applaydto)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(id, out var Id))
            {
                return Unauthorized();
            }
            var leave = new Leaf
            {
                EmployeeId = Id,
                LeaveType = applaydto.LeaveType,
                StartDate = applaydto.StartDate,
                EndDate = applaydto.EndDate,
                Reason = applaydto.Reason,
                Status = "Pending"
            };
            context.Leaves.Add(leave);
            context.SaveChanges();

            return Ok(ToResponseLeafDto(leave));
        }
        #endregion

        #region UpdateLeave
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:long}")]
        public IActionResult UpdateLeave(long id, UpdateLeaveDto updateLeafDto)
        {
            var leave = context.Leaves.FirstOrDefault(item => item.Id == id);
            if (leave == null)
            {
                return NotFound();
            }

            if (!context.Users.Any(user => user.Id == updateLeafDto.EmployeeId))
            {
                return NotFound("Employee not found.");
            }

            leave.EmployeeId = updateLeafDto.EmployeeId;
            leave.LeaveType = updateLeafDto.LeaveType;
            leave.StartDate = updateLeafDto.StartDate;
            leave.EndDate = updateLeafDto.EndDate;
            leave.Reason = updateLeafDto.Reason;
            leave.Status = updateLeafDto.Status;

            context.SaveChanges();

            return Ok(ToResponseLeafDto(leave));
        }
        #endregion

        #region ApproveLeave
        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("{id:long}/approve")]
        public IActionResult ApproveLeave(long id)
        {
            var leave = context.Leaves.FirstOrDefault(item => item.Id == id);
            if (leave == null)
            {
                return NotFound();
            }
            leave.Status = "Approved";
            context.SaveChanges();

            return Ok(ToResponseLeafDto(leave));
        }
        #endregion

        #region RejectLeave
        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("{id:long}/reject")]
        public IActionResult RejectLeave(long id)
        {
            var leave = context.Leaves.FirstOrDefault(item => item.Id == id);
            if (leave == null)
            {
                return NotFound();
            }
            leave.Status = "Rejected";
            context.SaveChanges();

            return Ok(ToResponseLeafDto(leave));
        }
        #endregion

        #region DeleteLeave
        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id:long}")]
        public IActionResult DeleteLeave(long id)
        {
            var leave = context.Leaves.FirstOrDefault(item => item.Id == id);
            if (leave == null)
            {
                return NotFound();
            }

            context.Leaves.Remove(leave);
            context.SaveChanges();

            return NoContent();
        }
        #endregion

        #region ResponseMapping
        private static ResponseLeaveDto ToResponseLeafDto(Leaf leave)
        {
            return new ResponseLeaveDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                LeaveType = leave.LeaveType,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status
            };
        }
        #endregion

    }
}