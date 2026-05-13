using Employee_Task_and_Attendance_Management_System.DTOs.Leaves;
using Employee_Task_and_Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [Authorize(Roles ="Admin,Manager")]
        [HttpGet]
        public IActionResult GetLeaves()
        {
            var leaves = context.Leaves.Select(leave => new ResponseLeafDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                LeaveType = leave.LeaveType,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status
            }).ToList();

            return Ok(leaves);
        }
        #endregion

        #region GetLeaveSelf
        [HttpGet("self")]
        public IActionResult GetLeaveById()
        {
            var Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!int.TryParse(Id,out var eid))
            {
                return Unauthorized();
            }

            var leave = context.Leaves.Where(item => item.EmployeeId == eid).Select(item => new ResponseLeafDto
            {
                Id = item.Id,
                EmployeeId = item.EmployeeId,
                LeaveType = item.LeaveType,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Reason = item.Reason,
                Status = item.Status
            });

            if (leave == null)
            {
                return NotFound();
            }

            return Ok(leave);
        }
        #endregion

        #region GetLeaveById
        [Authorize(Roles ="Admmin,Manager")]
        [HttpGet("{id}")]
        public IActionResult GetLeaveById(long id)
        {
            var leave = context.Leaves.Select(item => new ResponseLeafDto
            {
                Id = item.Id,
                EmployeeId = item.EmployeeId,
                LeaveType = item.LeaveType,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Reason = item.Reason,
                Status = item.Status
            }).FirstOrDefault(item => item.Id == id);

            if (leave == null)
            {
                return NotFound();
            }

            return Ok(leave);
        }
        #endregion

        #region CreateLeave
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult CreateLeave(CreateLeafDto createLeafDto)
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

            var response = new ResponseLeafDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                LeaveType = leave.LeaveType,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status
            };

            return Ok(response);
        }
        #endregion

        #region ApplyForLeave
        [HttpPost("apply")]
        public IActionResult ApplyForLeave(ApplayLeaveDto applaydto)
        {
            var id=User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!int.TryParse(id, out var Id))
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

            var response = new ResponseLeafDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                LeaveType = leave.LeaveType,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status
            };
            return Ok(response);
        }
        #endregion

        #region UpdateLeave
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateLeave(long id, UpdateLeafDto updateLeafDto)
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

            var response = new ResponseLeafDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                LeaveType = leave.LeaveType,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status
            };

            return Ok(response);
        }
        #endregion

        #region ApproveLeave
        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("{id}/approve")]
        public IActionResult ApproveLeave(int id) {
            var leave = context.Leaves.FirstOrDefault(item => item.Id == id);
            if (leave == null)
            {
                return NotFound();
            }
            leave.Status = "Approved";
            context.SaveChanges();

            var response = new ResponseLeafDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                LeaveType = leave.LeaveType,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status
            };

            return Ok(response);
        }
        #endregion

        #region RejectLeave
        [Authorize(Roles = "Admin,Manager")]
        [HttpPatch("{id}/reject")]
        public IActionResult RejectLeave(int id)
        {
            var leave = context.Leaves.FirstOrDefault(item => item.Id == id);
            if (leave == null)
            {
                return NotFound();
            }
            leave.Status = "Rejected";
            context.SaveChanges();

            var response = new ResponseLeafDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                LeaveType = leave.LeaveType,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                Reason = leave.Reason,
                Status = leave.Status
            };

            return Ok(response);
        }
        #endregion

        #region DeleteLeave
        [Authorize(Roles = "Admin,Employee")]
        [HttpDelete("{id}")]
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
    }
}