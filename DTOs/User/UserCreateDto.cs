using Employee_Task_and_Attendance_Management_System.Models;

namespace Employee_Task_and_Attendance_Management_System.DTOs.User
{
    public class UserCreateDto
    {
        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = "Employee";

        public int? DepartmentId { get; set; }

    }
}
