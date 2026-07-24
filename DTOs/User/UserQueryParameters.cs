using Employee_Task_and_Attendance_Management_System.DTOs.Common;

namespace Employee_Task_and_Attendance_Management_System.DTOs.User
{
    public class UserQueryParameters : QueryParameters
    {
        public string? Role { get; set; }
        public int? DepartmentId { get; set; }
    }
}
