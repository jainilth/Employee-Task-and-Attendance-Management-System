namespace Employee_Task_and_Attendance_Management_System.DTOs.Tasks
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int? AssignedTo { get; set; }

        public string Priority { get; set; } = null!;

        public DateTime? Deadline { get; set; }
    }
}