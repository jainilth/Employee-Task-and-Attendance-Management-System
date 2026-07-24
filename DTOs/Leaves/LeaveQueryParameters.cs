using Employee_Task_and_Attendance_Management_System.DTOs.Common;

namespace Employee_Task_and_Attendance_Management_System.DTOs.Leaves
{
    public class LeaveQueryParameters : QueryParameters
    {
        public string? Status { get; set; }
        public string? LeaveType { get; set; }
        public int? EmployeeId { get; set; }
    }
}
