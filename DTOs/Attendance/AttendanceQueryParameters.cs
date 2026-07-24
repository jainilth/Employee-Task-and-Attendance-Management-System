using System;
using Employee_Task_and_Attendance_Management_System.DTOs.Common;

namespace Employee_Task_and_Attendance_Management_System.DTOs.Attendance
{
    public class AttendanceQueryParameters : QueryParameters
    {
        public string? Status { get; set; }
        public DateOnly? Date { get; set; }
        public int? EmployeeId { get; set; }
    }
}
