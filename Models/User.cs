using System;
using System.Collections.Generic;

namespace Employee_Task_and_Attendance_Management_System.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? DepartmentId { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Leaf> Leaves { get; set; } = new List<Leaf>();

    public virtual ICollection<Task> TaskAssignedByNavigations { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskAssignedToNavigations { get; set; } = new List<Task>();
}
