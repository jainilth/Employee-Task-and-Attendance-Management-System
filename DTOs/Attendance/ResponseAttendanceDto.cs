namespace Employee_Task_and_Attendance_Management_System.DTOs.Attendance
{
    public class ResponseAttendanceDto
    {
        public long Id { get; set; }

        public int EmployeeId { get; set; }

        public DateTime CheckIn { get; set; }

        public DateTime? CheckOut { get; set; }

        public decimal? WorkingHours { get; set; }

        public string Status { get; set; } = null!;

        public DateOnly Date { get; set; }
    }
}