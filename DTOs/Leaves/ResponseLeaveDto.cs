namespace Employee_Task_and_Attendance_Management_System.DTOs.Leaves
{
    public class ResponseLeaveDto
    {
        public long Id { get; set; }

        public int EmployeeId { get; set; }

        public string LeaveType { get; set; } = null!;

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public string? Reason { get; set; }

        public string Status { get; set; } = null!;
    }
}