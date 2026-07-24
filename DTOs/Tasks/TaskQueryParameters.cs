using Employee_Task_and_Attendance_Management_System.DTOs.Common;

namespace Employee_Task_and_Attendance_Management_System.DTOs.Tasks
{
    public class TaskQueryParameters : QueryParameters
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public int? AssignedTo { get; set; }
    }
}
