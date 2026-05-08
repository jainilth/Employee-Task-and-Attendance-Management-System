using System;
using System.Collections.Generic;

namespace Employee_Task_and_Attendance_Management_System.Models;

public partial class Attendance
{
    public long Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public decimal? WorkingHours { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly Date { get; set; }

    public virtual User Employee { get; set; } = null!;
}
