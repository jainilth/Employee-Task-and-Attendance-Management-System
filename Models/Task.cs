using System;
using System.Collections.Generic;

namespace Employee_Task_and_Attendance_Management_System.Models;

public partial class Task
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int AssignedTo { get; set; }

    public int AssignedBy { get; set; }

    public string Priority { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? Deadline { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User AssignedByNavigation { get; set; } = null!;

    public virtual User AssignedToNavigation { get; set; } = null!;
}
