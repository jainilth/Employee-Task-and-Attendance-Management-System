namespace Employee_Task_and_Attendance_Management_System.DTOs.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Role { get; set; } = null!;

        public int? DepartmentId { get; set; }
    }
}