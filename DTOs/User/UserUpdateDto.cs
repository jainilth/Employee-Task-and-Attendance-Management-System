namespace Employee_Task_and_Attendance_Management_System.DTOs.User
{
    public class UserUpdateDto
    {
        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = null!;

        public int? DepartmentId { get; set; }
    }
}