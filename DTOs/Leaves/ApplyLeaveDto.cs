namespace Employee_Task_and_Attendance_Management_System.DTOs.Leaves
{
    public class ApplyLeaveDto
    {
        public string LeaveType { get; set; } = null!;

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public string? Reason { get; set; }

    }
}
